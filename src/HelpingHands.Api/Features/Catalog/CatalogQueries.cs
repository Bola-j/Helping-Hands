using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<IResult> CurrentCatalog(HelpingHandsDbContext db, string? cursor, int? limit, CancellationToken ct)
    {
        const string scope = "current";
        if (!TryCatalogCursor(cursor, scope, limit, out var after)) return Results.Problem(statusCode: 400, title: "Invalid pagination.");
        var cursorId = after.GetValueOrDefault();
        var now = DateTimeOffset.UtcNow;
        var rows = await (from c in db.ServiceCategories.AsNoTracking()
            join s in db.ServiceVersions.AsNoTracking() on c.Id equals s.CategoryId
            join u in db.UnitStandards.AsNoTracking() on s.Id equals u.ServiceVersionId
            join v in db.StandardVersions.AsNoTracking() on u.Id equals v.UnitStandardId
            where s.IsPublished && v.IsPublished && v.EffectiveAt <= now && (after == null || u.Id.CompareTo(cursorId) > 0)
                && !db.ServiceVersions.Any(newer => newer.CategoryId == c.Id && newer.IsPublished && newer.VersionNumber > s.VersionNumber)
                && !db.StandardVersions.Any(newer => newer.UnitStandardId == u.Id && newer.IsPublished && newer.EffectiveAt <= now && (newer.EffectiveAt > v.EffectiveAt || (newer.EffectiveAt == v.EffectiveAt && newer.VersionNumber > v.VersionNumber)))
            orderby u.Id
            select new { Category = c, Service = s, Standard = u, Version = v }).Take((limit ?? 25) + 1).ToListAsync(ct);
        var page = CatalogPageResult(rows, limit ?? 25, scope, x => x.Standard.Id);
        var ids = page.Items.Select(x => x.Version.Id).ToArray();
        var components = await db.StandardComponents.AsNoTracking().Where(x => ids.Contains(x.StandardVersionId)).OrderBy(x => x.Name).ThenBy(x => x.Id).ToListAsync(ct);
        var items = page.Items.Select(x => new CatalogChoice(x.Category.Id, x.Category.Code, x.Service.Id, x.Service.VersionNumber, x.Service.Name, x.Standard.Id, x.Standard.Name, x.Version.Id, x.Version.VersionNumber, x.Version.EffectiveAt, components.Where(c => c.StandardVersionId == x.Version.Id).Select(c => new CatalogComponent(c.Id, c.Name, c.Quantity, c.Unit)).ToArray())).ToArray();
        return Results.Ok(new CatalogPage<CatalogChoice>(items, page.NextCursor));
    }

    private static async Task<IResult> Categories(HelpingHandsDbContext db, string? cursor, int? limit, CancellationToken ct)
    {
        const string scope = "categories";
        if (!TryCatalogCursor(cursor, scope, limit, out var after)) return Results.Problem(statusCode: 400, title: "Invalid pagination.");
        var cursorId = after.GetValueOrDefault();
        var rows = await db.ServiceCategories.AsNoTracking().Where(x => after == null || x.Id.CompareTo(cursorId) > 0).OrderBy(x => x.Id).Take((limit ?? 25) + 1).ToListAsync(ct);
        return Results.Ok(CatalogPageResult(rows, limit ?? 25, scope, x => x.Id));
    }

    private static async Task<IResult> ServiceVersions(Guid id, HelpingHandsDbContext db, string? cursor, int? limit, CancellationToken ct)
    {
        var scope = $"service-versions:{id:N}";
        if (!TryCatalogCursor(cursor, scope, limit, out var after)) return Results.Problem(statusCode: 400, title: "Invalid pagination.");
        var cursorId = after.GetValueOrDefault();
        var rows = await db.ServiceVersions.AsNoTracking().Where(x => x.CategoryId == id && (after == null || x.Id.CompareTo(cursorId) > 0)).OrderBy(x => x.Id).Take((limit ?? 25) + 1).ToListAsync(ct);
        return Results.Ok(CatalogPageResult(rows, limit ?? 25, scope, x => x.Id));
    }
}
