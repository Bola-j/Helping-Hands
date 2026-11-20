using System.Text;
using HelpingHands.Api.Data;
using HelpingHands.Api.Email;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class CairoCapacityReminderRunner
{
    private async Task SendAndRecordAsync(
        StaleCharityTarget charity, DateOnly date, string recipient, Guid attemptId,
        RunProgress progress, CancellationToken cancellationToken)
    {
        var message = BuildReminderEmail(charity, date, recipient);
        var result = await SendEmailAsync(message, attemptId, cancellationToken);
        await RecordSendResultAsync(charity, date, attemptId, result, progress, cancellationToken);
    }

    private async Task RecordSendResultAsync(
        StaleCharityTarget charity, DateOnly date, Guid attemptId, SmtpSendResult result,
        RunProgress progress, CancellationToken cancellationToken)
    {
        if (result.Outcome == SmtpSendOutcome.AcceptedByRelay)
            await RecordAcceptedAsync(attemptId, progress, cancellationToken);
        else if (result.Outcome == SmtpSendOutcome.Ambiguous)
            await RecordAmbiguousAsync(charity, date, attemptId, result, progress, cancellationToken);
        else
            await RecordFailureAsync(charity, date, attemptId, result, progress, cancellationToken);
    }

    private static EmailMessage BuildReminderEmail(StaleCharityTarget charity, DateOnly date, string recipient)
    {
        var body = new StringBuilder();
        body.AppendLine($"Hello {charity.DisplayName},");
        body.AppendLine();
        body.AppendLine($"This is your daily capacity reminder for {date:yyyy-MM-dd} (Cairo local time).");
        body.AppendLine("The following enabled services currently have stale or missing capacity for today:");
        foreach (var service in charity.StaleServices) body.AppendLine($"- {service}");
        body.AppendLine();
        body.AppendLine("Please update your daily capacity so your services remain eligible for matching.");
        return new EmailMessage(recipient, $"Daily Capacity Reminder - {date:yyyy-MM-dd} (Cairo)", body.ToString());
    }
}
