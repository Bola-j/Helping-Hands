namespace HelpingHands.Api.Features.Catalog;

public sealed record CategoryInput(string? Code);
public sealed record NameInput(string? Name);
public sealed record StandardVersionInput(DateTimeOffset EffectiveAt);
public sealed record ComponentInput(string? Name, decimal Quantity, string? Unit);
