using HelpingHands.Api.Data;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class CairoCapacityReminderRunner
{
    public async Task<ReminderRunSummary> RunDueRemindersAsync(
        bool forceRunNow = false, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var date = DailyReminderStateService.GetCairoDate(now);
        var scheduled = DailyReminderStateService.GetScheduledUtcForCairoDay(date, DailyReminderLocalTime);
        var charities = await FindStaleCharitiesAsync(database, date, cancellationToken);
        if (!forceRunNow && now < scheduled)
            return await RunNotDueAsync(charities, date, scheduled, cancellationToken);
        var progress = new RunProgress();
        await ProcessCharitiesAsync(charities, date, now, progress, cancellationToken);
        return BuildSummary(date, scheduled, charities.Count, progress, 0);
    }

    private async Task<ReminderRunSummary> RunNotDueAsync(
        IReadOnlyList<StaleCharityTarget> charities, DateOnly date,
        DateTimeOffset scheduled, CancellationToken cancellationToken)
    {
        var created = await EnsureNotDueIntentsAsync(charities, date, scheduled, cancellationToken);
        var progress = new RunProgress { IntentsCreated = created };
        return BuildSummary(date, scheduled, charities.Count, progress, charities.Count);
    }

    private static ReminderRunSummary BuildSummary(
        DateOnly date, DateTimeOffset scheduled, int staleCount, RunProgress progress, int notDue) =>
        new(date, scheduled, staleCount, progress.IntentsCreated, progress.DeliveredCount,
            progress.FailedCount, progress.AmbiguousCount, progress.SkippedAlreadyDeliveredCount,
            progress.SkippedInProgressCount, notDue);
}
