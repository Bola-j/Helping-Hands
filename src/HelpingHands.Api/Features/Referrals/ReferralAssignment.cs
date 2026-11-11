using System.Security.Claims;
using HelpingHands.Api.Data;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<Referral> BuildReferral(HelpingHandsDbContext database, Guid requestId,
        Guid recipientId, ReferralInput input, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return new Referral
        {
            RequestId = requestId,
            RecipientId = recipientId,
            AssignedQuantity = input.AssignedQuantity,
            ScopeDescription = input.ScopeDescription.Trim(),
            CreatedByUserId = Actor(user),
            Reference = await database.NextReferenceAsync("REF", cancellationToken)
        };
    }

    private static void RecordReferralAssignment(HelpingHandsDbContext database, AssistanceRequest request,
        Referral referral, ClaimsPrincipal user)
    {
        database.Referrals.Add(referral);
        database.DecisionHistory.Add(new DecisionHistory
        {
            ReferralId = referral.Id,
            ActorUserId = Actor(user),
            Action = "Assigned"
        });
        request.Status = "Open";
        request.Version = Guid.NewGuid();
    }
}
