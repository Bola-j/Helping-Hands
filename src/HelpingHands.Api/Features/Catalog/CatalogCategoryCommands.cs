using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<IResult> CreateCategory(CategoryInput input, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct)
    {
        if (InvalidText(input.Code, 50)) return Results.Problem(statusCode: 400, title: "A category code is required.");
        var category = new ServiceCategory { Code = input.Code!.Trim() };
        db.ServiceCategories.Add(category);
        return await SaveCatalogAsync(db, ct) ?? CatalogResult(response, category, $"/api/v1/catalog/categories/{category.Id}");
    }

    private static async Task<IResult> CreateServiceVersion(Guid id, NameInput input, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct)
    {
        if (InvalidText(input.Name, 200)) return Results.Problem(statusCode: 400, title: "A service name is required.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var category = await db.ServiceCategories.FromSqlInterpolated($"SELECT * FROM \"ServiceCategory\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (category is null) return Results.Problem(statusCode: 404, title: "Catalog item not found.");
        var number = (await db.ServiceVersions.Where(x => x.CategoryId == id).MaxAsync(x => (int?)x.VersionNumber, ct) ?? 0) + 1;
        var version = new ServiceVersion { CategoryId = id, Name = input.Name!.Trim(), VersionNumber = number };
        db.ServiceVersions.Add(version);
        var error = await SaveCatalogAsync(db, ct);
        if (error is not null) return error;
        await tx.CommitAsync(ct);
        return CatalogResult(response, version, $"/api/v1/catalog/service-versions/{version.Id}");
    }
}
