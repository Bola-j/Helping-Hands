using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Needs;

public sealed partial class NeedVersionService
{
    private async Task<List<CatalogSelection>> GetSelectionsAsync(IReadOnlyList<NeedSelection> needs, CancellationToken ct)
    {
        var ids = needs.Select(x => x.StandardVersionId).ToArray();
        return await (from version in db.StandardVersions.AsNoTracking()
            join standard in db.UnitStandards.AsNoTracking() on version.UnitStandardId equals standard.Id
            join service in db.ServiceVersions.AsNoTracking() on standard.ServiceVersionId equals service.Id
            where ids.Contains(version.Id) && version.IsPublished && service.IsPublished
            select new CatalogSelection(version.Id, standard.Id, service.Id, service.CategoryId)).ToListAsync(ct);
    }

    private async Task LockCatalogAsync(List<CatalogSelection> selections, CancellationToken ct)
    {
        foreach (var id in selections.Select(x => x.CategoryId).Distinct().Order())
            await db.ServiceCategories.FromSqlInterpolated($"SELECT * FROM \"ServiceCategory\" WHERE \"Id\" = {id} FOR UPDATE").AsNoTracking().SingleAsync(ct);
        foreach (var id in selections.Select(x => x.UnitStandardId).Distinct().Order())
            await db.UnitStandards.FromSqlInterpolated($"SELECT * FROM \"UnitStandard\" WHERE \"Id\" = {id} FOR UPDATE").AsNoTracking().SingleAsync(ct);
    }

    private async Task<bool> AreCurrentAsync(List<CatalogSelection> selections, DateTimeOffset at, CancellationToken ct)
    {
        foreach (var selection in selections)
        {
            var service = await db.ServiceVersions.AsNoTracking().Where(x => x.CategoryId == selection.CategoryId && x.IsPublished).OrderByDescending(x => x.VersionNumber).Select(x => x.Id).FirstOrDefaultAsync(ct);
            var version = await db.StandardVersions.AsNoTracking().Where(x => x.UnitStandardId == selection.UnitStandardId && x.IsPublished && x.EffectiveAt <= at).OrderByDescending(x => x.EffectiveAt).ThenByDescending(x => x.VersionNumber).Select(x => x.Id).FirstOrDefaultAsync(ct);
            if (service != selection.ServiceVersionId || version != selection.StandardVersionId) return false;
        }
        return true;
    }
}
