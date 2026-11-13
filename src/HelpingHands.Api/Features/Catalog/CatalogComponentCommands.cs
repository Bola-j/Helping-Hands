using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<IResult> CreateComponent(Guid id, ComponentInput input, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct)
    {
        if (InvalidComponent(input)) return Results.Problem(statusCode: 400, title: "Component name, positive quantity and unit are required.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var version = await db.StandardVersions.FromSqlInterpolated($"SELECT * FROM \"StandardVersion\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (version is null) return Results.Problem(statusCode: 404, title: "Catalog item not found.");
        if (version.IsPublished) return Results.Problem(statusCode: 409, title: "Published components cannot be edited.");
        if (await db.StandardComponents.CountAsync(x => x.StandardVersionId == id, ct) >= 100) return Results.Problem(statusCode: 400, title: "A standard version may contain at most 100 components.");
        var component = new StandardComponent { StandardVersionId = id, Name = input.Name!.Trim(), Quantity = input.Quantity, Unit = input.Unit!.Trim() };
        db.StandardComponents.Add(component);
        var error = await SaveCatalogAsync(db, ct);
        if (error is not null) return error;
        await tx.CommitAsync(ct);
        return CatalogResult(response, component, $"/api/v1/catalog/components/{component.Id}");
    }

    private static async Task<IResult> UpdateComponent(Guid id, ComponentInput input, HttpRequest request, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct)
    {
        if (InvalidComponent(input)) return Results.Problem(statusCode: 400, title: "Component name, positive quantity and unit are required.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var component = await LockComponentAsync(id, db, ct);
        if (component is null) return Results.Problem(statusCode: 404, title: "Catalog item not found.");
        if (CheckTag(request, component) is { } stale) return stale;
        if (await db.StandardVersions.AnyAsync(x => x.Id == component.StandardVersionId && x.IsPublished, ct)) return Results.Problem(statusCode: 409, title: "Published components cannot be edited.");
        component.Name = input.Name!.Trim(); component.Quantity = input.Quantity; component.Unit = input.Unit!.Trim();
        var error = await SaveCatalogAsync(db, ct);
        if (error is not null) return error;
        await tx.CommitAsync(ct);
        return CatalogResult(response, component);
    }

    private static async Task<IResult> DeleteComponent(Guid id, HttpRequest request, HelpingHandsDbContext db, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var component = await LockComponentAsync(id, db, ct);
        if (component is null) return Results.Problem(statusCode: 404, title: "Catalog item not found.");
        if (CheckTag(request, component) is { } stale) return stale;
        if (await db.StandardVersions.AnyAsync(x => x.Id == component.StandardVersionId && x.IsPublished, ct)) return Results.Problem(statusCode: 409, title: "Published components cannot be edited.");
        db.StandardComponents.Remove(component);
        var error = await SaveCatalogAsync(db, ct);
        if (error is not null) return error;
        await tx.CommitAsync(ct);
        return Results.NoContent();
    }
}
