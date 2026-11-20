using HelpingHands.Api.Email;
using HelpingHands.Api.Features.Reminders;

namespace HelpingHands.Api.Tests;

public sealed class CairoCapacityReminderTests
{
    [Fact]
    public void ScheduledUtcForCairo8Am_HandlesWinterAndSummerOffsets()
    {
        // Winter date in Cairo (UTC+2) -> 08:00 Cairo = 06:00 UTC
        var winterDate = new DateOnly(2026, 1, 15);
        var winterUtc = DailyReminderStateService.GetScheduledUtcForCairoDay(winterDate, CairoCapacityReminderRunner.DailyReminderLocalTime);
        Assert.Equal(new DateTimeOffset(2026, 1, 15, 6, 0, 0, TimeSpan.Zero), winterUtc);

        // Summer date in Cairo when DST applies (UTC+3) -> 08:00 Cairo = 05:00 UTC
        var summerDate = new DateOnly(2026, 7, 15);
        var summerUtc = DailyReminderStateService.GetScheduledUtcForCairoDay(summerDate, CairoCapacityReminderRunner.DailyReminderLocalTime);
        var cairoOffset = DailyReminderStateService.CairoTimeZone.GetUtcOffset(summerDate.ToDateTime(new TimeOnly(8, 0), DateTimeKind.Unspecified));
        var expectedSummerUtc = new DateTimeOffset(summerDate.ToDateTime(new TimeOnly(8, 0), DateTimeKind.Unspecified), cairoOffset).ToUniversalTime();
        Assert.Equal(expectedSummerUtc, summerUtc);
    }

    [Fact]
    public void SmtpSendResult_PreservesExplicitOutcomesAndNeverClaimsMailboxDelivery()
    {
        var accepted = SmtpSendResult.RelayAccepted("localhost", 1025);
        Assert.Equal(SmtpSendOutcome.AcceptedByRelay, accepted.Outcome);
        Assert.False(accepted.ClaimedMailboxDelivery);

        var failed = SmtpSendResult.Failed("SMTP_CONN", "Connection refused");
        Assert.Equal(SmtpSendOutcome.Failed, failed.Outcome);
        Assert.False(failed.ClaimedMailboxDelivery);

        var ambiguous = SmtpSendResult.Ambiguous("SMTP_TIMEOUT", "Timeout after DATA command");
        Assert.Equal(SmtpSendOutcome.Ambiguous, ambiguous.Outcome);
        Assert.False(ambiguous.ClaimedMailboxDelivery);
    }
}