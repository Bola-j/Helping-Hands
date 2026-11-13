namespace HelpingHands.Api.Features.Catalog;

public sealed record CatalogChoice(Guid CategoryId, string CategoryCode, Guid ServiceVersionId, int ServiceVersionNumber, string ServiceName, Guid UnitStandardId, string StandardName, Guid StandardVersionId, int StandardVersionNumber, DateTimeOffset EffectiveAt, IReadOnlyList<CatalogComponent> Components);
public sealed record CatalogComponent(Guid Id, string Name, decimal Quantity, string Unit);

public sealed record CatalogVersionView(Guid Id, Guid UnitStandardId, int VersionNumber, bool IsPublished, DateTimeOffset EffectiveAt, IReadOnlyList<CatalogComponent> Components);
