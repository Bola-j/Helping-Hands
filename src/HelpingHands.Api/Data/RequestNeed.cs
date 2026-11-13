namespace HelpingHands.Api.Data;

public sealed class RequestNeed
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RequestId { get; set; }
    public Guid StandardVersionId { get; set; }
    public decimal Quantity { get; set; }
    public string Status { get; set; } = "Submitted";
}
