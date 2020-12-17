using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HeatKeeper.Reporter.Sdk
{
    public class RTL433Reporter : Reporter
    {
        private readonly Dictionary<string, Sensor> sensors = new Dictionary<string, Sensor>();

        // Contains the last measurements per sensor
        private readonly ConcurrentDictionary<string, Measurement[]> cache = new ConcurrentDictionary<string, Measurement[]>();

        private readonly HttpClient httpClient = new HttpClient();


        internal override async Task Start()
        {
            httpClient.BaseAddress = new Uri(this.EndpointConfiguration.Url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.EndpointConfiguration.ApiKey);
            await Task.WhenAny(StartRtl(), PublishMeasurements());
        }


        private async Task PublishMeasurements()
        {
            while (true)
            {
                foreach (var item in cache)
                {
                    Console.WriteLine($"Publishing {item.Value.Count()} measurements");
                    var content = new JsonContent(item.Value);
                    Console.WriteLine(content);
                    var response = await httpClient.PostAsync("api/measurements", content);
                    response.EnsureSuccessStatusCode();
                }
                await Task.Delay(publishIntervall);
            }
        }

        public RTL433Reporter AddSensor(Sensor sensor)
        {
            sensors.Add(sensor.Model, sensor);
            return this;
        }

        private async Task StartRtl()
        {
            var protocolArguments = sensors.Select(s => $"-R {s.Value.ProtocolId}").Aggregate((current, next) => $"{current} {next}");
            var arguments = $"-F json -M protocol -M utc {protocolArguments}";
            await Command.ExecuteAsync("rtl_433", arguments, ProcessData);
        }

        private void ProcessData(string data)
        {
            var document = System.Text.Json.JsonDocument.Parse(data);

            var model = document.RootElement.GetProperty("model").GetString();
            if (sensors.TryGetValue(model, out var sensor))
            {
                var id = document.RootElement.GetProperty("id").GetRawText();
                var measurements = new MeasurementFactory().CreateMeasurements(document.RootElement, sensor);
                cache.AddOrUpdate(id, i => measurements, (i, m) => measurements);
            }
            else
            {
                Console.Error.WriteLine($"Unknown model {model}");
            }
        }
    }

}