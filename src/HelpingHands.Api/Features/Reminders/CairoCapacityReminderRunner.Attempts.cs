using HelpingHands.Api.Data;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class CairoCapacityReminderRunner
{
    private async Task StartAndSendAsync(
        StaleCharityTarget charity, DateOnly date, DailyReminderIntent intent,
        RunProgress progress, CancellationToken cancellationToken)
    {
        var recipient = ChooseRecipientEmail(charity);
        var result = await stateService.BeginDeliveryAttemptAsync(intent.IntentId, recipient, cancellationToken);
        if (!result.CanProceed || result.Attempt is null) { CountSkipped(result, progress); return; }
        await SendAndRecordAsync(charity, date, recipient, result.Attempt.AttemptId, progress, cancellationToken);
    }

    private static string ChooseRecipientEmail(StaleCharityTarget charity) =>
        string.IsNullOrWhiteSpace(charity.ContactDetails)
            ? $"charity-{charity.RecipientId:N}@placeholder.helpinghands.local"
            : charity.ContactDetails.Trim();

    private static void CountSkipped(BeginAttemptResult result, RunProgress progress)
    {
        if (result.Reason == "AttemptInProgress") progress.SkippedInProgressCount++;
        else progress.SkippedAlreadyDeliveredCount++;
    }
}
