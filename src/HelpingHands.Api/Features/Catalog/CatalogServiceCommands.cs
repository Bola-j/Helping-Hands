using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<IResult> UpdateServiceVersion(Guid id, NameInput input, HttpRequest request, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct)
    {
        if (InvalidText(input.Name, 200)) return Results.Problem(statusCode: 400, title: "A service name is required.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var version = await db.ServiceVersions.FromSqlInterpolated($"SELECT * FROM \"ServiceVersion\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (version is null) return Results.Problem(statusCode: 404, title: "Catalog item not found.");
        if (CheckTag(request, version) is { } stale) return stale;
        if (version.IsPublished) return Results.Problem(statusCode: 409, title: "Published service versions cannot be edited.");
        version.Name = input.Name!.Trim();
        var error = await SaveCatalogAsync(db, ct);
        if (error is not null) return error;
        await tx.CommitAsync(ct);
        return CatalogResult(response, version);
    }

    private static async Task<IResult> PublishServiceVersion(Guid id, HttpRequest request, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var version = await db.ServiceVersions.FromSqlInterpolated($"SELECT * FROM \"ServiceVersion\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (version is null) return Results.Problem(statusCode: 404, title: "Catalog item not found.");
        if (CheckTag(request, version) is { } stale) return stale;
        if (version.IsPublished) return Results.Problem(statusCode: 409, title: "Service version is already published.");
        version.IsPublished = true;
        var error = await SaveCatalogAsync(db, ct);
        if (error is not null) return error;
        await tx.CommitAsync(ct);
        return CatalogResult(response, version);
    }
}
