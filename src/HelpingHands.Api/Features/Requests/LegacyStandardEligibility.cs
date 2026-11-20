using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static Task<bool> IsEligibleLegacyStandard(HelpingHandsDbContext database, Guid standardVersionId,
        DateTimeOffset at, CancellationToken cancellationToken) =>
        (from standard in database.StandardVersions.AsNoTracking()
         join unit in database.UnitStandards.AsNoTracking() on standard.UnitStandardId equals unit.Id
         join service in database.ServiceVersions.AsNoTracking() on unit.ServiceVersionId equals service.Id
         where standard.Id == standardVersionId && standard.IsPublished && standard.EffectiveAt <= at && service.IsPublished
         select standard.Id).AnyAsync(cancellationToken);
}
