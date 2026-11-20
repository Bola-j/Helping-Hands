namespace HelpingHands.Api.Features.Reminders;

public sealed partial class DailyReminderStateService
{
    public static readonly TimeZoneInfo CairoTimeZone = ResolveCairoTimeZone();

    private static TimeZoneInfo ResolveCairoTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"); }
    }

    public static DateOnly GetCairoDate(DateTimeOffset utcNow) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(utcNow, CairoTimeZone).DateTime);

    public static DateTimeOffset GetScheduledUtcForCairoDay(DateOnly cairoDate, TimeOnly? localTime = null)
    {
        var local = cairoDate.ToDateTime(localTime ?? new TimeOnly(8, 0), DateTimeKind.Unspecified);
        return new DateTimeOffset(local, CairoTimeZone.GetUtcOffset(local)).ToUniversalTime();
    }
}
