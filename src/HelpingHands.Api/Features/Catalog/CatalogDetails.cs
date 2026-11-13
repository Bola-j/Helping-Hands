using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<IResult> GetCategory(Guid id, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct) =>
        CatalogDetail(await db.ServiceCategories.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct), response);
    private static async Task<IResult> GetServiceVersion(Guid id, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct) =>
        CatalogDetail(await db.ServiceVersions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct), response);
    private static async Task<IResult> GetStandard(Guid id, HttpResponse response, HelpingHandsDbContext db, CancellationToken ct) =>
        CatalogDetail(await db.UnitStandards.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct), response);
}
