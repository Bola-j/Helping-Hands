using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<IResult> GetStandardVersion(Guid id, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var version = await db.StandardVersions.FromSqlInterpolated($"SELECT * FROM \"StandardVersion\" WHERE \"Id\" = {id} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (version is null) return Results.Problem(statusCode: 404, title: "Catalog item not found.");
        var result = await CatalogVersionResultAsync(response, version, db, ct);
        await tx.CommitAsync(ct);
        return result;
    }
    private static async Task<IResult> GetComponent(Guid id, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct) =>
        CatalogDetail(await db.StandardComponents.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct), response);
    private static IResult CatalogDetail<T>(T? item, HttpResponse response) where T : class =>
        item is null ? Results.Problem(statusCode: 404, title: "Catalog item not found.") : CatalogResult(response, item);
}
