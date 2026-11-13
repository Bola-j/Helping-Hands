using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Needs;

public sealed partial class NeedVersionService
{
    public async Task<IReadOnlyList<RecordedNeed>> GetRecordedNeedsAsync(Guid requestId, CancellationToken ct = default) =>
        (await GetRecordedNeedsForRequestsAsync([requestId], ct)).GetValueOrDefault(requestId) ?? [];

    public async Task<Dictionary<Guid, IReadOnlyList<RecordedNeed>>> GetRecordedNeedsForRequestsAsync(Guid[] requestIds, CancellationToken ct = default)
    {
        var needs = await (from need in db.RequestNeeds.AsNoTracking()
            join version in db.StandardVersions.AsNoTracking() on need.StandardVersionId equals version.Id
            join standard in db.UnitStandards.AsNoTracking() on version.UnitStandardId equals standard.Id
            join service in db.ServiceVersions.AsNoTracking() on standard.ServiceVersionId equals service.Id
            join category in db.ServiceCategories.AsNoTracking() on service.CategoryId equals category.Id
            where requestIds.Contains(need.RequestId)
            select new { Need = need, StandardVersionNumber = version.VersionNumber, version.EffectiveAt,
                UnitStandardId = standard.Id, StandardName = standard.Name, ServiceVersionId = service.Id,
                ServiceVersionNumber = service.VersionNumber, ServiceName = service.Name, CategoryId = category.Id, CategoryCode = category.Code }).ToListAsync(ct);
        var ids = needs.Select(x => x.Need.StandardVersionId).Distinct().ToArray();
        var components = await db.StandardComponents.AsNoTracking().Where(x => ids.Contains(x.StandardVersionId)).OrderBy(x => x.Name).ToListAsync(ct);
        return needs.Select(x => new RecordedNeed(x.Need.Id, x.Need.RequestId, x.Need.StandardVersionId, x.StandardVersionNumber,
            x.EffectiveAt, x.UnitStandardId, x.StandardName, x.ServiceVersionId, x.ServiceVersionNumber, x.ServiceName,
            x.CategoryId, x.CategoryCode, x.Need.Quantity, x.Need.Status, x.Need.Category, x.Need.ServiceSubtype,
            components.Where(c => c.StandardVersionId == x.Need.StandardVersionId).Select(c => new RecordedComponent(c.Name, c.Quantity, c.Unit)).ToList()))
            .GroupBy(x => x.RequestId).ToDictionary(x => x.Key, x => (IReadOnlyList<RecordedNeed>)x.ToList());
    }
}
