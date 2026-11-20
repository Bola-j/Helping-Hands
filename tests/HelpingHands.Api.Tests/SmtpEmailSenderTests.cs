using System.Net;
using System.Net.Sockets;
using System.Text;
using HelpingHands.Api.Configuration;
using HelpingHands.Api.Email;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HelpingHands.Api.Tests;

public sealed class SmtpEmailSenderTests
{
    [Fact]
    public async Task SendAsync_AcceptedByCatcher_ReturnsAcceptedByRelayWithoutClaimingMailboxDelivery()
    {
        await using var catcher = new FakeSmtpCatcher(FakeSmtpBehavior.AcceptMessage);
        var sender = CreateSender(new SmtpOptions
        {
            Host = "127.0.0.1",
            Port = catcher.Port,
            Username = "fictional-user",
            Password = "fictional-password",
            FromAddress = "reminders@helping-hands.example.test",
            UseSsl = false
        });

        var result = await sender.SendAsync(new EmailMessage(
            ToAddress: "coordinator@charity.example.test",
            Subject: "Daily capacity freshness reminder",
            PlainTextBody: "Please confirm your charity services capacity for today."));

        Assert.True(result.AcceptedByRelay, $"Expected relay acceptance but got {result.Outcome}: {result.ErrorCode} - {result.FailureReason}");
        Assert.Equal(SmtpSendOutcome.AcceptedByRelay, result.Outcome);
        Assert.False(result.ClaimedMailboxDelivery);
        Assert.Contains("not claimed", result.StatusSummary, StringComparison.OrdinalIgnoreCase);
        Assert.Null(result.ErrorCode);
        Assert.Null(result.FailureReason);
        Assert.Contains("MAIL FROM:<reminders@helping-hands.example.test>", catcher.Transcript);
        Assert.Contains("RCPT TO:<coordinator@charity.example.test>", catcher.Transcript);
        Assert.Contains("Subject: Daily capacity freshness reminder", catcher.Transcript);
    }

    [Fact]
    public async Task SendAsync_RecipientRejectedBySmtpRelay_ReturnsFailedResultWithoutThrowing()
    {
        await using var catcher = new FakeSmtpCatcher(FakeSmtpBehavior.RejectRecipient);
        var sender = CreateSender(new SmtpOptions
        {
            Host = "127.0.0.1",
            Port = catcher.Port,
            FromAddress = "reminders@helping-hands.example.test",
            UseSsl = false
        });

        var result = await sender.SendAsync(new EmailMessage(
            ToAddress: "rejected@charity.example.test",
            Subject: "Daily reminder",
            PlainTextBody: "Test body"));

        Assert.False(result.AcceptedByRelay);
        Assert.Equal(SmtpSendOutcome.Failed, result.Outcome);
        Assert.False(result.ClaimedMailboxDelivery);
        Assert.NotNull(result.ErrorCode);
        Assert.NotNull(result.FailureReason);
    }

    [Fact]
    public async Task SendAsync_ConnectionDroppedMidData_ReturnsAmbiguousOutcome()
    {
        await using var catcher = new FakeSmtpCatcher(FakeSmtpBehavior.DropAfterDataPayload);
        var sender = CreateSender(new SmtpOptions
        {
            Host = "127.0.0.1",
            Port = catcher.Port,
            FromAddress = "reminders@helping-hands.example.test",
            UseSsl = false
        });

        var result = await sender.SendAsync(new EmailMessage(
            ToAddress: "coordinator@charity.example.test",
            Subject: "Daily reminder",
            PlainTextBody: "Test body"));

        Assert.False(result.AcceptedByRelay);
        Assert.Equal(SmtpSendOutcome.Ambiguous, result.Outcome);
        Assert.False(result.ClaimedMailboxDelivery);
        Assert.NotNull(result.FailureReason);
    }

    [Fact]
    public async Task SendAsync_UnreachablePort_ReturnsFailedResult()
    {
        var unusedPort = GetUnusedLoopbackPort();
        var sender = CreateSender(new SmtpOptions
        {
            Host = "127.0.0.1",
            Port = unusedPort,
            FromAddress = "reminders@helping-hands.example.test",
            UseSsl = false
        });

        var result = await sender.SendAsync(new EmailMessage(
            ToAddress: "coordinator@charity.example.test",
            Subject: "Daily reminder",
            PlainTextBody: "Test body"));

        Assert.False(result.AcceptedByRelay);
        Assert.Equal(SmtpSendOutcome.Failed, result.Outcome);
        Assert.False(result.ClaimedMailboxDelivery);
        Assert.NotNull(result.ErrorCode);
    }

    [Fact]
    public async Task SendAsync_InvalidConfigurationOrRecipient_ReturnsFailedResultBeforeNetwork()
    {
        var missingHostSender = CreateSender(new SmtpOptions
        {
            Host = "",
            Port = 1025,
            FromAddress = "reminders@helping-hands.example.test"
        });

        var configResult = await missingHostSender.SendAsync(new EmailMessage(
            ToAddress: "coordinator@charity.example.test",
            Subject: "Subject",
            PlainTextBody: "Body"));

        Assert.Equal(SmtpSendOutcome.Failed, configResult.Outcome);
        Assert.Equal("InvalidConfiguration", configResult.ErrorCode);
        Assert.False(configResult.ClaimedMailboxDelivery);

        var validHostSender = CreateSender(new SmtpOptions
        {
            Host = "127.0.0.1",
            Port = 1025,
            FromAddress = "reminders@helping-hands.example.test"
        });

        var recipientResult = await validHostSender.SendAsync(new EmailMessage(
            ToAddress: "not-a-valid-email",
            Subject: "Subject",
            PlainTextBody: "Body"));

        Assert.Equal(SmtpSendOutcome.Failed, recipientResult.Outcome);
        Assert.Equal("InvalidRecipient", recipientResult.ErrorCode);
        Assert.False(recipientResult.ClaimedMailboxDelivery);
    }

    private static SmtpEmailSender CreateSender(SmtpOptions options) =>
        new(Options.Create(options), NullLogger<SmtpEmailSender>.Instance);

    private static int GetUnusedLoopbackPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private enum FakeSmtpBehavior
    {
        AcceptMessage,
        RejectRecipient,
        DropAfterDataPayload
    }

    private sealed class FakeSmtpCatcher : IAsyncDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _serverTask;
        private readonly StringBuilder _transcript = new();

        public int Port { get; }
        public string Transcript => _transcript.ToString();

        public FakeSmtpCatcher(FakeSmtpBehavior behavior)
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _serverTask = RunAsync(behavior, _cts.Token);
        }

        private async Task RunAsync(FakeSmtpBehavior behavior, CancellationToken ct)
        {
            try
            {
                using var client = await _listener.AcceptTcpClientAsync(ct);
                await using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
                await using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true)
                {
                    NewLine = "\r\n",
                    AutoFlush = true
                };

                await writer.WriteLineAsync("220 localhost FakeMailpit ESMTP");

                var inData = false;
                while (!ct.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(ct);
                    if (line is null)
                        break;

                    _transcript.AppendLine(line);

                    if (inData)
                    {
                        if (line == ".")
                        {
                            inData = false;
                            if (behavior == FakeSmtpBehavior.DropAfterDataPayload)
                            {
                                client.Close();
                                break;
                            }

                            await writer.WriteLineAsync("250 2.0.0 Ok: queued in local catcher");
                        }

                        continue;
                    }

                    if (line.StartsWith("EHLO", StringComparison.OrdinalIgnoreCase) ||
                        line.StartsWith("HELO", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("250-localhost");
                        await writer.WriteLineAsync("250-AUTH PLAIN");
                        await writer.WriteLineAsync("250 OK");
                    }
                    else if (line.StartsWith("AUTH PLAIN", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("235 2.7.0 Authentication successful");
                    }
                    else if (line.StartsWith("AUTH LOGIN", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("334 VXNlcm5hbWU6");
                        var user = await reader.ReadLineAsync(ct);
                        _transcript.AppendLine(user);
                        await writer.WriteLineAsync("334 UGFzc3dvcmQ6");
                        var pass = await reader.ReadLineAsync(ct);
                        _transcript.AppendLine(pass);
                        await writer.WriteLineAsync("235 2.7.0 Authentication successful");
                    }
                    else if (line.StartsWith("MAIL FROM:", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("250 2.1.0 Ok");
                    }
                    else if (line.StartsWith("RCPT TO:", StringComparison.OrdinalIgnoreCase))
                    {
                        if (behavior == FakeSmtpBehavior.RejectRecipient)
                        {
                            await writer.WriteLineAsync("550 5.1.1 Recipient address rejected");
                        }
                        else
                        {
                            await writer.WriteLineAsync("250 2.1.5 Ok");
                        }
                    }
                    else if (line.Equals("DATA", StringComparison.OrdinalIgnoreCase))
                    {
                        inData = true;
                        await writer.WriteLineAsync("354 End data with <CR><LF>.<CR><LF>");
                    }
                    else if (line.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("221 2.0.0 Bye");
                        break;
                    }
                    else
                    {
                        await writer.WriteLineAsync("250 Ok");
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (IOException)
            {
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _cts.CancelAsync();
            _listener.Stop();
            try
            {
                await _serverTask;
            }
            catch
            {
            }
            _cts.Dispose();
        }
    }
}
