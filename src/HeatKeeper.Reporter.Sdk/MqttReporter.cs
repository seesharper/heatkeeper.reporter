using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace HeatKeeper.Reporter.Sdk;


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
        var content = new JsonContent(allMeasurements.ToArray());
        Console.WriteLine($"Publishing {allMeasurements.Count} measurements");
        var response = await httpClient.PostAsync("api/measurements", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine("Failed to post MQTT measurements");
        }
    }

    public MqttReporter AddSensor(MqttSensor mqttSensor)
    {
        _sensors.Add(mqttSensor);
        return this;
    }

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
                .WithClientId("HeatKeeper")
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

public class MqttSensor
{
    private readonly Func<MqttApplicationMessage, Task<Measurement[]>> _payloadHandler;

    public MqttSensor(string topic, Func<MqttApplicationMessage, Task<Measurement[]>> payloadHandler)
    {
        Topic = topic;
        _payloadHandler = payloadHandler;
    }

    public string Topic { get; }

    public async Task<Measurement[]> HandleMessage(MqttApplicationMessage applicationMessage)
    {
        return await _payloadHandler(applicationMessage);
    }
}

public static partial class MqttSensors
{
    public static MqttSensor ShellyPlusHT()
    {
        return new MqttSensor("shelly/plus-ht/#", async applicationMessage =>
        {
            if (applicationMessage.Topic.Contains("humidity", StringComparison.OrdinalIgnoreCase))
            {
                return new Measurement[] { ReadHumidity(applicationMessage.Topic, applicationMessage.ConvertPayloadToString()) };
            }

            if (applicationMessage.Topic.Contains("temperature", StringComparison.OrdinalIgnoreCase))
            {
                return new Measurement[] { ReadTemperature(applicationMessage.Topic, applicationMessage.ConvertPayloadToString()) };
            }

            if (applicationMessage.Topic.Contains("devicepower", StringComparison.OrdinalIgnoreCase))
            {
                return new Measurement[] { ReadBatteryLevel(applicationMessage.Topic, applicationMessage.ConvertPayloadToString()) };
            }

            throw new ArgumentOutOfRangeException("applicationMessage.Topic", applicationMessage.Topic, "Unknown topic");
        });

        static Measurement ReadTemperature(string topic, string payload)
        {
            JsonDocument document = JsonDocument.Parse(payload);
            var temperature = document.RootElement.GetProperty("tC").GetDouble();
            var sensorId = MyRegex().Match(topic).Groups[1].Value;
            return new Measurement(sensorId, MeasurementType.Temperature, RetentionPolicy.Day, temperature, DateTime.UtcNow);
        }

        static Measurement ReadHumidity(string topic, string payload)
        {
            JsonDocument document = JsonDocument.Parse(payload);
            var humidity = document.RootElement.GetProperty("rh").GetDouble();
            var sensorId = MyRegex().Match(topic).Groups[1].Value;
            return new Measurement(sensorId, MeasurementType.Humidity, RetentionPolicy.Day, humidity, DateTime.UtcNow);
        }

        static Measurement ReadBatteryLevel(string topic, string payload)
        {
            JsonDocument document = JsonDocument.Parse(payload);
            var batteryLevel = document.RootElement.GetProperty("battery").GetProperty("percent").GetDouble();
            var sensorId = MyRegex().Match(topic).Groups[1].Value;
            return new Measurement(sensorId, MeasurementType.BatteryLevel, RetentionPolicy.Day, batteryLevel, DateTime.UtcNow);
        }
    }

    [GeneratedRegex(@"^(.*)\/status.*$")]
    private static partial Regex MyRegex();
}
