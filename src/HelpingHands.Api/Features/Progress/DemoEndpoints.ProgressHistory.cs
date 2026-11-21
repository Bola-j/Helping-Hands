using System.Security.Claims;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Progress;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IReadOnlyList<ProgressHistoryEvent>> ProgressHistoryAsync(HelpingHandsDbContext database,
        AssistanceRequest request, IReadOnlyList<Referral> referrals, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var events = await RequestAndReferralHistoryAsync(database, request, referrals, user, cancellationToken);
        if (!user.IsInRole("RecipientRepresentative")) events.AddRange(await NeedHistoryAsync(database, request.Id, user, cancellationToken));
        if (Staff(user) || user.IsInRole("RecipientRepresentative"))
            events.AddRange(await OutcomeReviewHistoryAsync(database, referrals, user, cancellationToken));
        return events.OrderBy(item => item.OccurredAt).ThenBy(item => item.EntityId).ToList();
    }
}
