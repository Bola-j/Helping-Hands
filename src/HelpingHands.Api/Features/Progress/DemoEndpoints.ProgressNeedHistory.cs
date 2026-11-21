using System.Security.Claims;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Progress;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<List<ProgressHistoryEvent>> NeedHistoryAsync(HelpingHandsDbContext database, Guid requestId,
        ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var needIds = await database.RequestNeeds.AsNoTracking().Where(item => item.RequestId == requestId)
            .Select(item => item.Id).ToArrayAsync(cancellationToken);
        var rows = await database.NeedReviewHistory.AsNoTracking().Where(item => needIds.Contains(item.NeedId))
            .OrderBy(item => item.OccurredAt).ToListAsync(cancellationToken);
        return rows.Select(item => new ProgressHistoryEvent("Need", item.NeedId, item.Action, item.OccurredAt,
            Staff(user) ? item.ActorUserId : null, Staff(user) ? item.Reason : null)).ToList();
    }
}
