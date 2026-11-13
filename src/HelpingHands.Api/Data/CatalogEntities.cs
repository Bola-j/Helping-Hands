namespace HelpingHands.Api.Data;

public sealed class ServiceCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = "";
}

public sealed class ServiceVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CategoryId { get; set; }
    public int VersionNumber { get; set; }
    public string Name { get; set; } = "";
}

public sealed class UnitStandard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ServiceVersionId { get; set; }
    public string Name { get; set; } = "";
}

public sealed class StandardVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UnitStandardId { get; set; }
    public int VersionNumber { get; set; }
    public DateTimeOffset EffectiveAt { get; set; }
}

public sealed class StandardComponent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StandardVersionId { get; set; }
    public string Name { get; set; } = "";
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "";
}
