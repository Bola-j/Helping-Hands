namespace HelpingHands.Api.Email;

public sealed record EmailMessage(
    string ToAddress,
    string Subject,
    string PlainTextBody);

public enum SmtpSendOutcome
{
    AcceptedByRelay,
    Failed,
    Ambiguous
}

public sealed record SmtpSendResult(
    SmtpSendOutcome Outcome,
    bool ClaimedMailboxDelivery,
    string StatusSummary,
    string? ErrorCode = null,
    string? FailureReason = null)
{
    public bool AcceptedByRelay => Outcome == SmtpSendOutcome.AcceptedByRelay;

    public static SmtpSendResult RelayAccepted(string host, int port) =>
        new(
            Outcome: SmtpSendOutcome.AcceptedByRelay,
            ClaimedMailboxDelivery: false,
            StatusSummary: $"Accepted by SMTP relay {host}:{port}; end-to-end recipient mailbox delivery is not claimed.");

    public static SmtpSendResult Failed(string errorCode, string failureReason) =>
        new(
            Outcome: SmtpSendOutcome.Failed,
            ClaimedMailboxDelivery: false,
            StatusSummary: "SMTP relay submission failed; recipient mailbox delivery was not attempted or claimed.",
            ErrorCode: errorCode,
            FailureReason: failureReason);

    public static SmtpSendResult Ambiguous(string errorCode, string failureReason) =>
        new(
            Outcome: SmtpSendOutcome.Ambiguous,
            ClaimedMailboxDelivery: false,
            StatusSummary: "SMTP relay outcome is ambiguous after transmission; recipient mailbox delivery is not claimed.",
            ErrorCode: errorCode,
            FailureReason: failureReason);
}

public interface ISmtpEmailSender
{
    Task<SmtpSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}