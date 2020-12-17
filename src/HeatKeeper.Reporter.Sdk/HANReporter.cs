using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using HANReader.Core;
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

    public class EndpointConfiguration
    {
        public string Url { get; set; }

        public string ApiKey { get; set; }
    }




    public class ReporterHost
    {
        private EndpointConfiguration endpointConfiguration = new();

        private List<Reporter> reporters = new();

        public ReporterHost WithHeatKeeperEndpoint(string url, string apiKey)
        {
            endpointConfiguration.Url = url;
            endpointConfiguration.ApiKey = apiKey;
            return this;
        }

        public ReporterHost AddReporter(Reporter reporter)
        {
            reporter.EndpointConfiguration = endpointConfiguration;
            reporters.Add(reporter);
            return this;
        }

        public async Task Start()
        {
            await Task.WhenAll(reporters.Select(r => r.Start()));
        }
    }


    public class Command
    {
        public static async Task<int> ExecuteAsync(string commandPath, string arguments, Action<string> dataReceived, string workingDirectory = null)
        {
            var process = CreateProcess(commandPath, arguments, workingDirectory);
            RedirectOutput(process, dataReceived);
            var exitCode = await StartProcessAsync(process);
            if (exitCode != 0)
            {
                throw new InvalidOperationException($"The command {commandPath} {arguments} failed.");
            }
            return exitCode;
        }

        private static Task<int> StartProcessAsync(Process process)
        {
            var tcs = new TaskCompletionSource<int>();
            process.Exited += (o, s) => tcs.SetResult(process.ExitCode);
            process.EnableRaisingEvents = true;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return tcs.Task;
        }

        private static void RedirectOutput(Process process, Action<string> dataReceived)
        {
            process.OutputDataReceived += (o, a) => WriteStandardOut(a);
            process.ErrorDataReceived += (o, a) => WriteStandardError(a);
            void WriteStandardOut(DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    Console.Error.WriteLine(args.Data);
                    dataReceived(args.Data);
                }
            }

            void WriteStandardError(DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    Console.Error.WriteLine(args.Data);
                }
            }
        }

        private static Process CreateProcess(string commandPath, string arguments, string workingDirectory)
        {
            var startInformation = new ProcessStartInfo($"{commandPath}");
            startInformation.CreateNoWindow = true;
            startInformation.Arguments = arguments;
            startInformation.RedirectStandardOutput = true;
            startInformation.RedirectStandardError = true;
            startInformation.UseShellExecute = false;
            startInformation.WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory;
            var process = new Process();
            process.StartInfo = startInformation;
            return process;
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