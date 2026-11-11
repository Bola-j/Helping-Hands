using System.Security.Claims;
using HelpingHands.Api.Data;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static void RecordOutcomeReview(HelpingHandsDbContext database, Outcome outcome,
        Guid referralId, ReviewInput input, ClaimsPrincipal user)
    {
        outcome.Status = input.Decision;
        outcome.Version = Guid.NewGuid();
        database.OutcomeReviews.Add(new OutcomeReview
        {
            OutcomeId = outcome.Id,
            ActorUserId = Actor(user),
            Decision = input.Decision,
            Reason = input.Reason
        });
        database.DecisionHistory.Add(new DecisionHistory
        {
            ReferralId = referralId,
            ActorUserId = Actor(user),
            Action = "Outcome" + input.Decision,
            Reason = input.Reason
        });
    }
}
