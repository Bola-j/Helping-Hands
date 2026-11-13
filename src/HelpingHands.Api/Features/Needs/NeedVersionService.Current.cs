using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Needs;

public sealed partial class NeedVersionService
{
    // A preview is not a reservation. AddNeedsAsync rechecks it inside the caller's transaction.
    public async Task<Guid?> FindCurrentAsync(Guid categoryId, Guid unitStandardId, DateTimeOffset at, CancellationToken ct = default)
    {
        var service = await db.ServiceVersions.AsNoTracking().Where(x => x.CategoryId == categoryId && x.IsPublished)
            .OrderByDescending(x => x.VersionNumber).Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
        if (service is null || !await db.UnitStandards.AnyAsync(x => x.Id == unitStandardId && x.ServiceVersionId == service, ct)) return null;
        return await db.StandardVersions.AsNoTracking().Where(x => x.UnitStandardId == unitStandardId && x.IsPublished && x.EffectiveAt <= at)
            .OrderByDescending(x => x.EffectiveAt).ThenByDescending(x => x.VersionNumber).Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
    }
}
