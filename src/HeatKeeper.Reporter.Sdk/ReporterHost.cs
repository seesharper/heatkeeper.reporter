using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Janitor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace HeatKeeper.Reporter.Sdk
{
    public class ReporterHost
    {
        private readonly EndpointConfiguration endpointConfiguration = new();

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
            var hostBuilder = Host.CreateDefaultBuilder();
            _ = hostBuilder.ConfigureServices(async sc =>
            {
                sc.AddJanitor((sp, janitor) =>
                {
                    foreach (var reporter in reporters)
                    {
                        janitor.Schedule(builder =>
                        {
                            builder
                                .WithName(reporter.GetType().Name)
                                .WithSchedule(new TimeSpanSchedule(reporter.PublishInterval))
                                .WithScheduledTask(async (IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) => await reporter.PublishMeasurements(httpClientFactory.CreateClient(reporter.GetType().Name), cancellationToken));
                        });
                    }
                });

                foreach (var reporter in reporters)
                {
                    sc.AddHttpClient(reporter.GetType().Name, client =>
                    {
                        client.BaseAddress = new Uri(endpointConfiguration.Url);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", endpointConfiguration.ApiKey);
                    });
                }

                sc.AddHostedService<JanitorHostedService>();
                sc.AddHostedService(sp => new ReportersHostedService(reporters.ToArray()));

            });
            hostBuilder.Build().Run();

            // await Task.WhenAll(reporters.Select(r => r.Start()));
            // Console.Error.WriteLine("ReporterHost exited");
        }
    }
}