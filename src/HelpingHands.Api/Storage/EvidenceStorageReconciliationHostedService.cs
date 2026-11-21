using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpingHands.Api.Storage;

public sealed class EvidenceStorageReconciliationHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<EvidenceStorageOptions> options,
    ILogger<EvidenceStorageReconciliationHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(options.Value.CleanupIntervalMinutes));
        do
            await RunPassAsync(stoppingToken);
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunPassAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var reconciler = scope.ServiceProvider.GetRequiredService<EvidenceStorageReconciliationService>();
            var count = await reconciler.ReconcileAsync(stoppingToken);
            if (count > 0)
                logger.LogInformation("Removed {Count} orphaned evidence objects.", count);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception error)
        {
            logger.LogWarning(error, "Evidence storage reconciliation failed; a later pass will retry.");
        }
    }
}
