using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<IResult> UpdateStandardVersion(Guid id, StandardVersionInput input, HttpRequest request, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct)
    {
        if (input.EffectiveAt == default) return Results.Problem(statusCode: 400, title: "An effective timestamp is required.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var version = await db.StandardVersions.FromSqlInterpolated($"SELECT * FROM \"StandardVersion\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (version is null) return Results.Problem(statusCode: 404, title: "Catalog item not found.");
        if (await CheckVersionTagAsync(request, version, db, ct) is { } stale) return stale;
        if (version.IsPublished) return Results.Problem(statusCode: 409, title: "Published standard versions cannot be edited.");
        version.EffectiveAt = input.EffectiveAt.ToUniversalTime();
        var error = await SaveCatalogAsync(db, ct);
        if (error is not null) return error;
        var result = await CatalogVersionResultAsync(response, version, db, ct);
        await tx.CommitAsync(ct);
        return result;
    }

    private static async Task<IResult> PublishStandardVersion(Guid id, HttpRequest request, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var version = await db.StandardVersions.FromSqlInterpolated($"SELECT * FROM \"StandardVersion\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (version is null) return Results.Problem(statusCode: 404, title: "Catalog item not found.");
        if (await CheckVersionTagAsync(request, version, db, ct) is { } stale) return stale;
        if (version.IsPublished) return Results.Problem(statusCode: 409, title: "Standard version is already published.");
        if (!await db.StandardComponents.AnyAsync(x => x.StandardVersionId == id, ct)) return Results.Problem(statusCode: 400, title: "At least one component is required.");
        var servicePublished = await (from standard in db.UnitStandards join service in db.ServiceVersions on standard.ServiceVersionId equals service.Id where standard.Id == version.UnitStandardId select service.IsPublished).SingleAsync(ct);
        if (!servicePublished) return Results.Problem(statusCode: 409, title: "The service version must be published first.");
        version.IsPublished = true;
        var error = await SaveCatalogAsync(db, ct);
        if (error is not null) return error;
        var result = await CatalogVersionResultAsync(response, version, db, ct);
        await tx.CommitAsync(ct);
        return result;
    }
}
