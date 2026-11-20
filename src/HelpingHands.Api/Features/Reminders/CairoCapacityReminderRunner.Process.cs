namespace HelpingHands.Api.Features.Reminders;

public sealed partial class CairoCapacityReminderRunner
{
    private async Task ProcessCharitiesAsync(
        IReadOnlyList<StaleCharityTarget> charities, DateOnly date,
        DateTimeOffset now, RunProgress progress, CancellationToken cancellationToken)
    {
        foreach (var charity in charities)
            await ProcessCharityAsync(charity, date, now, progress, cancellationToken);
    }

    private async Task ProcessCharityAsync(
        StaleCharityTarget charity, DateOnly date, DateTimeOffset now,
        RunProgress progress, CancellationToken cancellationToken)
    {
        var ensured = await stateService.EnsureDailyIntentAsync(charity.RecipientId, date, cancellationToken: cancellationToken);
        if (ensured.Created) progress.IntentsCreated++;
        if (ensured.Intent.Status == "Delivered") { progress.SkippedAlreadyDeliveredCount++; return; }
        if (await HasRecentAmbiguousFailureAsync(ensured.Intent, now, cancellationToken))
        { progress.AmbiguousCount++; return; }
        await StartAndSendAsync(charity, date, ensured.Intent, progress, cancellationToken);
    }
}
