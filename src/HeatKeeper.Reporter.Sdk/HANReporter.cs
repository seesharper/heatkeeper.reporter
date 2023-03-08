using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HeatKeeper.Reporter.Sdk
{
    public abstract class Reporter
    {
        internal abstract Task Start();

        public TimeSpan PublishInterval { get; set; }

        internal EndpointConfiguration EndpointConfiguration { get; set; }

        public Reporter WithPublishInterval(TimeSpan publishInterval)
        {
            PublishInterval = publishInterval;
            return this;
        }

        public abstract Task PublishMeasurements(HttpClient httpClient, CancellationToken cancellationToken);
    }


    public class HANReporter : Reporter
    {

        private ConcurrentQueue<Measurement[]> queue = new ConcurrentQueue<Measurement[]>();

        private string serialPort;

        internal override async Task Start()
        {
            await StartHANReader();
        }

        public HANReporter WithSerialPort(string serialPort)
        {
            this.serialPort = serialPort;
            return this;
        }

        public override async Task PublishMeasurements(HttpClient httpClient, CancellationToken cancellationToken)
        {
            var allMeasurements = new List<Measurement>();
            while (queue.TryDequeue(out var measurements))
            {
                allMeasurements.AddRange(measurements);
            }
            var content = new JsonContent(allMeasurements.ToArray());
            Console.WriteLine($"Publishing {allMeasurements.Count} measurements");
            var response = await httpClient.PostAsync("api/measurements", content);
            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine("Failed to post HAN measurements");
            }
        }

        private async Task StartHANReader()
        {
            await Command.ExecuteAsync("hanreader-linux-arm", serialPort, (data) => ProcessData(data));
        }

        private void ProcessData(string data)
        {
            KaifaMeasurementsFactory measurementsFactory = new KaifaMeasurementsFactory();
            var document = JsonDocument.Parse(data);
            var measurements = measurementsFactory.CreateMeasurements(document.RootElement);
            queue.Enqueue(measurements);
        }
    }
}