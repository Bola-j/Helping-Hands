namespace HelpingHands.Api.Data;

public sealed class DailyReminderIntent
{
    public Guid IntentId { get; set; } = Guid.NewGuid();
    public Guid CharityId { get; set; }
    public DateOnly CairoDate { get; set; }
    public DateTimeOffset ScheduledForUtc { get; set; }
    public string Status { get; set; } = "Pending";
    public int AttemptCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastAttemptAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public Guid Version { get; set; } = Guid.NewGuid();
}

public sealed class ReminderDeliveryAttempt
{
    public Guid AttemptId { get; set; } = Guid.NewGuid();
    public Guid IntentId { get; set; }
    public int AttemptNumber { get; set; }
    public string Status { get; set; } = "InProgress";
    public string? RecipientEmail { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
}