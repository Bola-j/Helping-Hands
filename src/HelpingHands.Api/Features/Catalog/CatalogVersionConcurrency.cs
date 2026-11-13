using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<CatalogVersionView> VersionRepresentationAsync(StandardVersion version, HelpingHandsDbContext db, CancellationToken ct)
    {
        var components = await db.StandardComponents.AsNoTracking().Where(x => x.StandardVersionId == version.Id).OrderBy(x => x.Id).ToListAsync(ct);
        return new CatalogVersionView(version.Id, version.UnitStandardId, version.VersionNumber, version.IsPublished, version.EffectiveAt, components.Select(x => new CatalogComponent(x.Id, x.Name, x.Quantity, x.Unit)).ToArray());
    }

    private static async Task<IResult?> CheckVersionTagAsync(HttpRequest request, StandardVersion version, HelpingHandsDbContext db, CancellationToken ct) =>
        CheckTag(request, await VersionRepresentationAsync(version, db, ct));

    private static async Task<IResult> CatalogVersionResultAsync(HttpResponse response, StandardVersion version, HelpingHandsDbContext db, CancellationToken ct, string? location = null)
    {
        var view = await VersionRepresentationAsync(version, db, ct);
        response.Headers.ETag = CatalogTag(view);
        return location is null ? Results.Ok(view) : Results.Created(location, view);
    }
}
