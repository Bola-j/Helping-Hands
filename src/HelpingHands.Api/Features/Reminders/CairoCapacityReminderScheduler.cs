using HelpingHands.Api.Data;
using HelpingHands.Api.Email;
using Microsoft.Extensions.Logging;

namespace HelpingHands.Api.Features.Reminders;

public sealed record ReminderRunSummary(
    DateOnly CairoDate,
    DateTimeOffset ScheduledForUtc,
    int StaleCharityCount,
    int IntentsCreated,
    int DeliveredCount,
    int FailedCount,
    int AmbiguousCount,
    int SkippedAlreadyDeliveredCount,
    int SkippedInProgressCount,
    int SkippedNotDueCount);

public sealed partial class CairoCapacityReminderRunner(
    HelpingHandsDbContext database,
    DailyReminderStateService stateService,
    ISmtpEmailSender emailSender,
    TimeProvider timeProvider,
    ILogger<CairoCapacityReminderRunner> logger)
{
    public static readonly TimeOnly DailyReminderLocalTime = new(8, 0);

    private sealed class RunProgress
    {
        public int IntentsCreated { get; set; }
        public int DeliveredCount { get; set; }
        public int FailedCount { get; set; }
        public int AmbiguousCount { get; set; }
        public int SkippedAlreadyDeliveredCount { get; set; }
        public int SkippedInProgressCount { get; set; }
    }

    internal sealed record StaleCharityTarget(
        Guid RecipientId, string DisplayName, string? ContactDetails, IReadOnlyList<string> StaleServices);

    private sealed record CharityGroupKey(Guid CharityId, string DisplayName, string? ContactDetails);

    private sealed record EnabledService(
        Guid CharityId, string DisplayName, string? ContactDetails, Guid CharityServiceId,
        string ServiceName, int VersionNumber, int StandardVersionNumber, string UnitName);
}
