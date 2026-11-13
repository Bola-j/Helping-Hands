using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<IResult> Standards(Guid id, HelpingHandsDbContext db, string? cursor, int? limit, CancellationToken ct)
    {
        var scope = $"standards:{id:N}";
        if (!TryCatalogCursor(cursor, scope, limit, out var after)) return Results.Problem(statusCode: 400, title: "Invalid pagination.");
        var cursorId = after.GetValueOrDefault();
        var rows = await db.UnitStandards.AsNoTracking().Where(x => x.ServiceVersionId == id && (after == null || x.Id.CompareTo(cursorId) > 0)).OrderBy(x => x.Id).Take((limit ?? 25) + 1).ToListAsync(ct);
        return Results.Ok(CatalogPageResult(rows, limit ?? 25, scope, x => x.Id));
    }

    private static async Task<IResult> StandardVersions(Guid id, HelpingHandsDbContext db, string? cursor, int? limit, CancellationToken ct)
    {
        var scope = $"standard-versions:{id:N}";
        if (!TryCatalogCursor(cursor, scope, limit, out var after)) return Results.Problem(statusCode: 400, title: "Invalid pagination.");
        var cursorId = after.GetValueOrDefault();
        var rows = await db.StandardVersions.AsNoTracking().Where(x => x.UnitStandardId == id && (after == null || x.Id.CompareTo(cursorId) > 0)).OrderBy(x => x.Id).Take((limit ?? 25) + 1).ToListAsync(ct);
        var page = CatalogPageResult(rows, limit ?? 25, scope, x => x.Id);
        var ids = page.Items.Select(x => x.Id).ToArray();
        var components = await db.StandardComponents.AsNoTracking().Where(x => ids.Contains(x.StandardVersionId)).OrderBy(x => x.Name).ThenBy(x => x.Id).ToListAsync(ct);
        var items = page.Items.Select(x => new { x.Id, x.UnitStandardId, x.VersionNumber, x.IsPublished, x.EffectiveAt, Components = components.Where(c => c.StandardVersionId == x.Id).Select(c => new CatalogComponent(c.Id, c.Name, c.Quantity, c.Unit)) }).ToArray();
        return Results.Ok(new { Items = items, page.NextCursor });
    }
}
