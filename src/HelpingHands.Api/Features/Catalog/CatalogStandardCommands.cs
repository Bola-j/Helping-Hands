using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<IResult> CreateStandard(Guid id, NameInput input, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct)
    {
        if (InvalidText(input.Name, 200)) return Results.Problem(statusCode: 400, title: "A standard name is required.");
        if (!await db.ServiceVersions.AnyAsync(x => x.Id == id, ct)) return Results.Problem(statusCode: 404, title: "Catalog item not found.");
        var standard = new UnitStandard { ServiceVersionId = id, Name = input.Name!.Trim() };
        db.UnitStandards.Add(standard);
        return await SaveCatalogAsync(db, ct) ?? CatalogResult(response, standard, $"/api/v1/catalog/standards/{standard.Id}");
    }

    private static async Task<IResult> UpdateStandard(Guid id, NameInput input, HttpRequest request, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct)
    {
        if (InvalidText(input.Name, 200)) return Results.Problem(statusCode: 400, title: "A standard name is required.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var standard = await db.UnitStandards.FromSqlInterpolated($"SELECT * FROM \"UnitStandard\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (standard is null) return Results.Problem(statusCode: 404, title: "Catalog item not found.");
        if (CheckTag(request, standard) is { } stale) return stale;
        if (await db.StandardVersions.AnyAsync(x => x.UnitStandardId == id && x.IsPublished, ct)) return Results.Problem(statusCode: 409, title: "A published standard cannot be renamed.");
        standard.Name = input.Name!.Trim();
        var error = await SaveCatalogAsync(db, ct);
        if (error is not null) return error;
        await tx.CommitAsync(ct);
        return CatalogResult(response, standard);
    }

    private static async Task<IResult> CreateStandardVersion(Guid id, StandardVersionInput input, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct)
    {
        if (input.EffectiveAt == default) return Results.Problem(statusCode: 400, title: "An effective timestamp is required.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var standard = await db.UnitStandards.FromSqlInterpolated($"SELECT * FROM \"UnitStandard\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (standard is null) return Results.Problem(statusCode: 404, title: "Catalog item not found.");
        var number = (await db.StandardVersions.Where(x => x.UnitStandardId == id).MaxAsync(x => (int?)x.VersionNumber, ct) ?? 0) + 1;
        var version = new StandardVersion { UnitStandardId = id, VersionNumber = number, EffectiveAt = input.EffectiveAt.ToUniversalTime() };
        db.StandardVersions.Add(version);
        var error = await SaveCatalogAsync(db, ct);
        if (error is not null) return error;
        var result = await CatalogVersionResultAsync(response, version, db, ct, $"/api/v1/catalog/standard-versions/{version.Id}");
        await tx.CommitAsync(ct);
        return result;
    }
}
