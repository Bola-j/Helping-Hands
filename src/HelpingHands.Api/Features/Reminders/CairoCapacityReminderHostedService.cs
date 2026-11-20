using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HelpingHands.Api.Features.Reminders;

public sealed class CairoCapacityReminderHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<CairoCapacityReminderHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunReminderTickAsync(stoppingToken);
            await DelayUntilNextTickAsync(stoppingToken);
        }
    }

    private async Task RunReminderTickAsync(CancellationToken token)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<CairoCapacityReminderRunner>();
            await runner.RunDueRemindersAsync(forceRunNow: false, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception error) { logger.LogError(error, "Error while executing scheduled 08:00 Cairo capacity reminders."); }
    }

    private static async Task DelayUntilNextTickAsync(CancellationToken token)
    {
        try { await Task.Delay(PollInterval, token); }
        catch (OperationCanceledException) { }
    }
}
