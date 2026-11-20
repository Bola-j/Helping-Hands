using HelpingHands.Api.Email;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class CairoCapacityReminderRunner
{
    private async Task<SmtpSendResult> SendEmailAsync(
        EmailMessage message, Guid attemptId, CancellationToken cancellationToken)
    {
        try { return await emailSender.SendAsync(message, cancellationToken); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await stateService.CompleteDeliveryAttemptAsync(
                attemptId, false, "Ambiguous: Delivery cancelled during send attempt.", CancellationToken.None);
            throw;
        }
        catch (Exception error)
        {
            return SmtpSendResult.Ambiguous("SEND_EXCEPTION",
                $"Ambiguous exception while sending reminder: {error.Message}");
        }
    }
}
