using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using HelpingHands.Api.Configuration;
using Microsoft.Extensions.Options;

namespace HelpingHands.Api.Email;

public sealed class SmtpEmailSender(
    IOptions<SmtpOptions> optionsAccessor,
    ILogger<SmtpEmailSender> logger) : ISmtpEmailSender
{
    private const int DefaultTimeoutMilliseconds = 10_000;

    public async Task<SmtpSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (cancellationToken.IsCancellationRequested)
            return SmtpSendResult.Failed("Cancelled", "Email send operation was cancelled before connecting to SMTP relay.");

        var options = optionsAccessor.Value;

        if (string.IsNullOrWhiteSpace(options.Host))
            return SmtpSendResult.Failed("InvalidConfiguration", "Smtp:Host is not configured.");

        if (options.Port is < 1 or > 65535)
            return SmtpSendResult.Failed("InvalidConfiguration", $"Smtp:Port '{options.Port}' is outside valid TCP port range.");

        if (!TryCreateMailAddress(options.FromAddress, out var fromAddress))
            return SmtpSendResult.Failed("InvalidConfiguration", "Smtp:FromAddress is missing or invalid.");

        if (!TryCreateMailAddress(message.ToAddress, out var toAddress))
            return SmtpSendResult.Failed("InvalidRecipient", "Recipient email address is missing or invalid.");

        if (string.IsNullOrWhiteSpace(message.Subject))
            return SmtpSendResult.Failed("InvalidMessage", "Email subject cannot be empty.");

        using var mail = new MailMessage(fromAddress!, toAddress!)
        {
            Subject = message.Subject.Trim(),
            Body = message.PlainTextBody ?? string.Empty,
            IsBodyHtml = false
        };

        using var client = CreateClient(options);

        try
        {
            await client.SendMailAsync(mail, cancellationToken);

            logger.LogInformation(
                "SMTP message accepted by relay {Host}:{Port} for recipient domain {RecipientDomain} (mailbox delivery not claimed).",
                options.Host,
                options.Port,
                toAddress!.Host);

            return SmtpSendResult.RelayAccepted(options.Host, options.Port);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("SMTP send to {Host}:{Port} cancelled while in flight; outcome is ambiguous.", options.Host, options.Port);
            return SmtpSendResult.Ambiguous(
                "CancelledInFlight",
                "Operation was cancelled while communicating with the SMTP relay; message acceptance is ambiguous.");
        }
        catch (SmtpException ex)
        {
            var isAmbiguous = IsAmbiguousSmtpFailure(ex);
            var code = ex.InnerException is SocketException sock
                ? $"SocketError_{sock.SocketErrorCode}"
                : $"SmtpStatus_{ex.StatusCode}";
            var reason = $"{ex.StatusCode}: {ex.Message}";

            logger.LogWarning(
                ex,
                "SMTP send to {Host}:{Port} ended with status {StatusCode} (ambiguous={IsAmbiguous}).",
                options.Host,
                options.Port,
                ex.StatusCode,
                isAmbiguous);

            return isAmbiguous
                ? SmtpSendResult.Ambiguous(code, reason)
                : SmtpSendResult.Failed(code, reason);
        }
        catch (SocketException ex)
        {
            logger.LogWarning(ex, "Socket error connecting to SMTP relay {Host}:{Port}.", options.Host, options.Port);
            return SmtpSendResult.Failed($"SocketError_{ex.SocketErrorCode}", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during SMTP relay submission to {Host}:{Port}.", options.Host, options.Port);
            return SmtpSendResult.Failed(ex.GetType().Name, ex.Message);
        }
    }

    private static SmtpClient CreateClient(SmtpOptions options)
    {
        var client = new SmtpClient(options.Host.Trim(), options.Port)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            EnableSsl = options.UseSsl,
            Timeout = DefaultTimeoutMilliseconds,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(options.Username))
        {
            client.Credentials = new NetworkCredential(options.Username, options.Password ?? string.Empty);
        }

        return client;
    }

    private static bool TryCreateMailAddress(string? value, out MailAddress? address)
    {
        address = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            address = new MailAddress(value.Trim());
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool IsAmbiguousSmtpFailure(SmtpException ex)
    {
        if (ex.InnerException is SocketException)
            return false;

        if (ex.InnerException is TimeoutException or IOException)
            return true;

        return ex.StatusCode is SmtpStatusCode.ServiceClosingTransmissionChannel
            or SmtpStatusCode.ServiceNotAvailable;
    }
}