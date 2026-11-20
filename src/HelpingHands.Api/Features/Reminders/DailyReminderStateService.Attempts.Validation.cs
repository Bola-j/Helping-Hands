namespace HelpingHands.Api.Features.Reminders;

public sealed partial class DailyReminderStateService
{
    private static bool IsValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 320) return false;
        try
        {
            var address = new System.Net.Mail.MailAddress(value);
            return address.Address.Equals(value, StringComparison.OrdinalIgnoreCase)
                && !address.Host.EndsWith(".local", StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException) { return false; }
    }
}
