namespace HelpingHands.Api.Features.Needs;

public sealed record NeedSelection(Guid StandardVersionId, decimal Quantity);
public enum NeedCreationResult { Created, InvalidRequest, InvalidNeeds, StaleSelection }
public sealed record RecordedNeed(Guid Id, Guid StandardVersionId, Guid ServiceVersionId, decimal Quantity, string Status, IReadOnlyList<RecordedComponent> Components);
public sealed record RecordedComponent(string Name, decimal Quantity, string Unit);
internal sealed record CatalogSelection(Guid StandardVersionId, Guid UnitStandardId, Guid ServiceVersionId, Guid CategoryId);
