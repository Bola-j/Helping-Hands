namespace HelpingHands.Api.Data;

public sealed class CharityService
{
    public Guid CharityServiceId { get; set; } = Guid.NewGuid();
    public Guid CharityId { get; set; }
    public Guid ServiceVersionId { get; set; }
    public Guid StandardVersionId { get; set; }
    public bool Enabled { get; set; } = true;
}

public sealed class DailyCapacity
{
    public Guid CapacityId { get; set; } = Guid.NewGuid();
    public Guid CharityServiceId { get; set; }
    public DateOnly CairoDate { get; set; }
    public decimal Available { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CapacityUpdate
{
    public Guid CapacityUpdateId { get; set; } = Guid.NewGuid();
    public Guid CapacityId { get; set; }
    public Guid ActorId { get; set; }
    public decimal Available { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}