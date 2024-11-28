using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace HeatKeeper.Reporter.Sdk;

/// <summary>
/// A reporter that reads measurements from MQTT and publishes them to the HeatKeeper API.
/// </summary>
public class MqttReporter : Reporter
{
    private string _brokerTcpAddress;
    private string _brokerUser;
    private string _brokerPassword;

    private readonly List<MqttSensor> _sensors = new();

    private readonly ConcurrentQueue<Measurement[]> _measurementsToPublish = new();

    public override async Task PublishMeasurements(HttpClient httpClient, CancellationToken cancellationToken)
    {
        var allMeasurements = new List<Measurement>();
        while (_measurementsToPublish.TryDequeue(out var measurements))
        {
            allMeasurements.AddRange(measurements);
        }
        if (allMeasurements.Count == 0)
        {
            Console.WriteLine("No measurements to publish");
            return;
        }

        var content = new JsonContent(allMeasurements.ToArray());
        Console.WriteLine($"Publishing {allMeasurements.Count} measurements");
        var response = await httpClient.PostAsync("api/measurements", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine("Failed to post MQTT measurements");
        }
    }

    /// <summary>
    /// Adds a new <see cref="MqttSensor"/> to the reporter.
    /// </summary>
    /// <param name="mqttSensor">The <see cref="MqttSensor"/> to be added.</param>
    /// <returns></returns>
    public MqttReporter AddSensor(MqttSensor mqttSensor)
    {
        _sensors.Add(mqttSensor);
        return this;
    }

    /// <summary>
    /// Sets the MQTT broker options.
    /// </summary>
    /// <param name="brokerTcpAddress">The tcp address of the broker. </param>
    /// <param name="brokerUser">The username used to connect to the broker.</param>
    /// <param name="brokerPassword">The password used to connect to the broker.</param>
    /// <returns>The <see cref="MqttReporter"/> for chaining calls.</returns>
    public MqttReporter WithMqttBrokerOptions(string brokerTcpAddress, string brokerUser, string brokerPassword)
    {
        _brokerTcpAddress = brokerTcpAddress;
        _brokerUser = brokerUser;
        _brokerPassword = brokerPassword;
        return this;
    }

    internal async override Task Start()
    {
        var options = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithClientId("HeatKeeper.Reporter")
                .WithCredentials(_brokerUser, _brokerPassword)
                .WithTcpServer(_brokerTcpAddress, 1883)
                .Build())
            .Build();
        IManagedMqttClient managedClient = new MqttFactory().CreateManagedMqttClient();
        await managedClient.StartAsync(options);

        var filters = _sensors.Select(s => new MqttTopicFilterBuilder().WithTopic(s.Topic).Build()).ToList();
        await managedClient.SubscribeAsync(filters);

        managedClient.ApplicationMessageReceivedAsync += async e =>
        {
            var sensor = _sensors.Single(s => e.ApplicationMessage.Topic.StartsWith(s.Topic.Replace("#", "")));
            var measurements = await sensor.HandleMessage(e.ApplicationMessage);
            _measurementsToPublish.Enqueue(measurements);
        };

    }
}


/// <summary>
/// Represents a sensor that can be read from MQTT.
/// </summary>
public class MqttSensor
{
    private readonly Func<MqttApplicationMessage, Task<Measurement[]>> _payloadHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttSensor"/> class.
    /// </summary>
    /// <param name="topic">The topic to subscribe to.</param>
    /// <param name="payloadHandler">The function used to handle the <see cref="MqttApplicationMessage"/>.</param>
    public MqttSensor(string topic, Func<MqttApplicationMessage, Task<Measurement[]>> payloadHandler)
    {
        Topic = topic;
        _payloadHandler = payloadHandler;
    }

    /// <summary>
    /// Gets the subscription topic.
    /// </summary>
    public string Topic { get; }

    /// <summary>
    /// Handles the <paramref name="applicationMessage"/> and returns the measurements.
    /// </summary>
    /// <param name="applicationMessage">The <see cref="MqttApplicationMessage"/> to be handled.</param>
    /// <returns>A list of measurements.</returns>
    public async Task<Measurement[]> HandleMessage(MqttApplicationMessage applicationMessage)
    {
        return await _payloadHandler(applicationMessage);
    }
}

public static partial class MqttSensors
{
    /// <summary>
    /// Creates a new <see cref="MqttSensor"/> for the Shelly Plus HT.
    /// </summary>
    /// <returns>A new MqttSensor for the Shelly Plus HT.</returns>
    public static MqttSensor ShellyPlusHT()
    {
        return new MqttSensor("shelly/plus-ht/#", async applicationMessage =>
        {
            JsonDocument document = JsonDocument.Parse(applicationMessage.ConvertPayloadToString());
            Console.WriteLine(document.RootElement.ToString());
            if (applicationMessage.Topic.EndsWith("/events/rpc", ignoreCase: true, culture: null))
            {
                var timestamp = DateTime.UtcNow;

                if (document.RootElement.GetProperty("method").GetString().Equals("NotifyFullStatus", StringComparison.OrdinalIgnoreCase))
                {
                    var temperature = document.RootElement.GetProperty("params").GetProperty("temperature:0").GetProperty("tC").GetDouble();
                    var humidity = document.RootElement.GetProperty("params").GetProperty("humidity:0").GetProperty("rh").GetDouble();
                    var batteryLevel = document.RootElement.GetProperty("params").GetProperty("devicepower:0").GetProperty("battery").GetProperty("percent").GetDouble();
                    var sensorId = applicationMessage.Topic.Replace("/events/rpc", "");

                    var temperatureMeasurement = new Measurement(sensorId, MeasurementType.Temperature, RetentionPolicy.Day, temperature, timestamp);
                    var humidityMeasurement = new Measurement(sensorId, MeasurementType.Humidity, RetentionPolicy.Day, humidity, timestamp);
                    var batteryLevelMeasurement = new Measurement(sensorId, MeasurementType.BatteryLevel, RetentionPolicy.Day, batteryLevel, timestamp);
                    return new Measurement[] { temperatureMeasurement, humidityMeasurement, batteryLevelMeasurement };
                }
            }

            return Array.Empty<Measurement>();
        });
    }

    public static MqttSensor DS18B20()
    {
        return new MqttSensor("tele/DS18B20/SENSOR", async applicationMessage =>
        {
            JsonDocument document = JsonDocument.Parse(applicationMessage.ConvertPayloadToString());
            Console.WriteLine(document.RootElement.ToString());


            var temperature = document.RootElement.GetProperty("DS18B20").GetProperty("Temperature").GetDouble();
            var timeStamp = DateTime.Parse(document.RootElement.GetProperty("Time").GetString());
            var utcTimeStamp = DateTime.SpecifyKind(timeStamp, DateTimeKind.Utc);
            var sensorId = document.RootElement.GetProperty("DS18B20").GetProperty("Id").GetString();

            var temperatureMeasurement = new Measurement(sensorId, MeasurementType.Temperature, RetentionPolicy.Day, temperature, utcTimeStamp);
            return [temperatureMeasurement];
        });
    }
}
