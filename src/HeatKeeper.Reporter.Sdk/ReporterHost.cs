using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace HeatKeeper.Reporter.Sdk
{
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
            Console.Error.WriteLine("ReporterHost exited");
        }
    }
}