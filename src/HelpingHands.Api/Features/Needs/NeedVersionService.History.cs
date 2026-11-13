using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Needs;

public sealed partial class NeedVersionService
{
    public async Task<IReadOnlyList<RecordedNeed>> GetRecordedNeedsAsync(Guid requestId, CancellationToken ct = default)
    {
        var needs = await (from need in db.RequestNeeds.AsNoTracking()
            join version in db.StandardVersions.AsNoTracking() on need.StandardVersionId equals version.Id
            join standard in db.UnitStandards.AsNoTracking() on version.UnitStandardId equals standard.Id
            where need.RequestId == requestId
            select new { need.Id, need.StandardVersionId, standard.ServiceVersionId, need.Quantity, need.Status }).ToListAsync(ct);
        var ids = needs.Select(x => x.StandardVersionId).ToArray();
        var components = await db.StandardComponents.AsNoTracking().Where(x => ids.Contains(x.StandardVersionId)).OrderBy(x => x.Name).ToListAsync(ct);
        return needs.Select(x => new RecordedNeed(x.Id, x.StandardVersionId, x.ServiceVersionId, x.Quantity, x.Status,
            components.Where(c => c.StandardVersionId == x.StandardVersionId).Select(c => new RecordedComponent(c.Name, c.Quantity, c.Unit)).ToList())).ToList();
    }
}
