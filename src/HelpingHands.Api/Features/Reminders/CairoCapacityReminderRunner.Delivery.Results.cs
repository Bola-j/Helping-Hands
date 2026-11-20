using HelpingHands.Api.Email;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class CairoCapacityReminderRunner
{
    private async Task RecordAcceptedAsync(Guid attemptId, RunProgress progress, CancellationToken cancellationToken)
    {
        var completion = await stateService.CompleteDeliveryAttemptAsync(
            attemptId, true, cancellationToken: cancellationToken);
        if (completion.Intent.Status == "Delivered") progress.DeliveredCount++;
        else progress.FailedCount++;
    }

    private async Task RecordAmbiguousAsync(
        StaleCharityTarget charity, DateOnly date, Guid attemptId, SmtpSendResult result,
        RunProgress progress, CancellationToken cancellationToken)
    {
        var reason = $"Ambiguous: {result.FailureReason ?? result.StatusSummary}";
        await stateService.CompleteDeliveryAttemptAsync(attemptId, false, reason, cancellationToken);
        progress.AmbiguousCount++;
        logger.LogWarning("Ambiguous SMTP outcome for charity {CharityId} on {CairoDate}: {Reason}",
            charity.RecipientId, date, reason);
    }

    private async Task RecordFailureAsync(
        StaleCharityTarget charity, DateOnly date, Guid attemptId, SmtpSendResult result,
        RunProgress progress, CancellationToken cancellationToken)
    {
        var reason = result.FailureReason ?? result.StatusSummary;
        await stateService.CompleteDeliveryAttemptAsync(attemptId, false, reason, cancellationToken);
        progress.FailedCount++;
        logger.LogWarning("Failed SMTP delivery for charity {CharityId} on {CairoDate}: {Reason}",
            charity.RecipientId, date, reason);
    }
}
