using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<StandardComponent?> LockComponentAsync(Guid id, HelpingHandsDbContext db, CancellationToken ct)
    {
        var versionId = await db.StandardComponents.AsNoTracking().Where(x => x.Id == id).Select(x => (Guid?)x.StandardVersionId).SingleOrDefaultAsync(ct);
        if (versionId is null) return null;
        await db.StandardVersions.FromSqlInterpolated($"SELECT * FROM \"StandardVersion\" WHERE \"Id\" = {versionId.Value} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(ct);
        return await db.StandardComponents.SingleOrDefaultAsync(x => x.Id == id && x.StandardVersionId == versionId, ct);
    }
}
