using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<IResult> UpdateCategory(Guid id, CategoryInput input, HttpRequest request, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct)
    {
        if (InvalidText(input.Code, 50)) return Results.Problem(statusCode: 400, title: "A category code is required.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var category = await db.ServiceCategories.FromSqlInterpolated($"SELECT * FROM \"ServiceCategory\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (category is null) return Results.Problem(statusCode: 404, title: "Catalog item not found.");
        if (CheckTag(request, category) is { } stale) return stale;
        if (await db.ServiceVersions.AnyAsync(x => x.CategoryId == id && x.IsPublished, ct)) return Results.Problem(statusCode: 409, title: "A published category cannot be edited.");
        category.Code = input.Code!.Trim();
        var error = await SaveCatalogAsync(db, ct);
        if (error is not null) return error;
        await tx.CommitAsync(ct);
        return CatalogResult(response, category);
    }
}
