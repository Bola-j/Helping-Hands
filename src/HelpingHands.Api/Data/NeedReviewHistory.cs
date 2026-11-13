namespace HelpingHands.Api.Data;

public sealed class NeedReviewHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NeedId { get; set; }
    public Guid ActorUserId { get; set; }
    public string Action { get; set; } = "";
    public string? Reason { get; set; }
    public string? Category { get; set; }
    public string? ServiceSubtype { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
