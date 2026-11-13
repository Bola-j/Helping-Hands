namespace HelpingHands.Api.Features.Needs;

public sealed record NeedSelection(Guid StandardVersionId, decimal Quantity);
public enum NeedCreationResult { Created, InvalidRequest, InvalidNeeds, StaleSelection }
public sealed record RecordedNeed(Guid Id, Guid RequestId, Guid StandardVersionId, int StandardVersionNumber,
    DateTimeOffset EffectiveAt, Guid UnitStandardId, string StandardName, Guid ServiceVersionId,
    int ServiceVersionNumber, string ServiceName, Guid CategoryId, string CategoryCode, decimal Quantity,
    string Status, string? Category, string? ServiceSubtype, IReadOnlyList<RecordedComponent> Components);
public sealed record RecordedComponent(string Name, decimal Quantity, string Unit);
internal sealed record CatalogSelection(Guid StandardVersionId, Guid UnitStandardId, Guid ServiceVersionId, Guid CategoryId);
