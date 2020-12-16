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
    public class RTL433Reporter
    {
        private TimeSpan publishIntervall = new TimeSpan(0, 0, 5);

        private readonly Dictionary<string, Sensor> sensors = new Dictionary<string, Sensor>();

        // Contains the last measurements per sensor
        private readonly ConcurrentDictionary<string, Measurement[]> cache = new ConcurrentDictionary<string, Measurement[]>();

        private readonly HttpClient httpClient = new HttpClient();

        private string apiKey;

        private string heatKeeperUrl;

        public void Start()
        {
            httpClient.BaseAddress = new Uri(heatKeeperUrl);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.apiKey);
            Task.WhenAny(Task.Run(() => StartRtl()), PublishMeasurements()).Result.Wait();
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


        public RTL433Reporter WithPublishInterval(TimeSpan publishIntervall)
        {
            this.publishIntervall = publishIntervall;
            return this;
        }

        public RTL433Reporter WithHeatKeeperEndpoint(string url, string apiKey)
        {
            this.heatKeeperUrl = url;
            this.apiKey = apiKey;
            return this;
        }

        public RTL433Reporter AddSensor(Sensor sensor)
        {
            sensors.Add(sensor.Model, sensor);
            return this;
        }

        private void StartRtl()
        {
            var protocolArguments = sensors.Select(s => $"-R {s.Value.ProtocolId}").Aggregate((current, next) => $"{current} {next}");

            var startInfo = new ProcessStartInfo();

            startInfo.FileName = "rtl_433";
            startInfo.Arguments = $"-F json -M protocol -M utc {protocolArguments}";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;

            var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.Exited += (sender, args) =>
            {
                throw new Exception("the rtl_433 process has exited");
            };
            process.ErrorDataReceived += (SetIndexBinder, args) =>
            {
                Console.Error.WriteLine(args.Data);
            };
            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data == null)
                {
                    return;
                }

                Console.Error.WriteLine(args.Data);


                var document = System.Text.Json.JsonDocument.Parse(args.Data);

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
            };
            process.WaitForExit();

        }
    }

}