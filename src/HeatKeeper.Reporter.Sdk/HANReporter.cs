using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace HeatKeeper.Reporter.Sdk
{
    public abstract class Reporter
    {
        internal abstract Task Start();

        protected TimeSpan publishIntervall;

        internal EndpointConfiguration EndpointConfiguration { get; set; }

        public Reporter WithPublishIntervall(TimeSpan publishIntervall)
        {
            this.publishIntervall = publishIntervall;
            return this;
        }

    }





    public class HANReporter : Reporter
    {

        private readonly HttpClient httpClient = new HttpClient();

        private ConcurrentQueue<Measurement[]> queue = new ConcurrentQueue<Measurement[]>();

        private string serialPort;

        internal override async Task Start()
        {
            httpClient.BaseAddress = new Uri(this.EndpointConfiguration.Url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.EndpointConfiguration.ApiKey);
            await Task.WhenAny(StartHANReader(), PublishMeasurements());
        }

        public HANReporter WithSerialPort(string serialPort)
        {
            this.serialPort = serialPort;
            return this;
        }

        private async Task PublishMeasurements()
        {
            while (true)
            {
                var allmeasurements = new List<Measurement>();
                while (queue.TryDequeue(out var measurements))
                {
                    allmeasurements.AddRange(measurements);
                }
                var content = new JsonContent(allmeasurements.ToArray());
                Console.WriteLine($"Publishing {allmeasurements.Count} measurements");
                var response = await httpClient.PostAsync("api/measurements", content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine("Failed to post HAN measurements");
                }

                await Task.Delay(publishIntervall);
            }
        }

        private async Task StartHANReader()
        {
            await Command.ExecuteAsync("hanreader-linux-arm", serialPort, (data) => ProcessData(data));
        }

        private void ProcessData(string data)
        {
            HANMeasurementsFactory measurementsFactory = new HANMeasurementsFactory();
            var document = JsonDocument.Parse(data);
            var measurements = measurementsFactory.CreateMeasurements(document.RootElement);
            queue.Enqueue(measurements);
        }
    }
}