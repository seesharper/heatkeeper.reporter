using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace HeatKeeper.Reporter.Sdk;


public class ReportersHostedService : BackgroundService
{
    private readonly Reporter[] reporters;

    public ReportersHostedService(Reporter[] reporters)
    {
        this.reporters = reporters;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(reporters.Select(r => r.Start()));
    }
}