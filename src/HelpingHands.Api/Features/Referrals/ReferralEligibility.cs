using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static bool InvalidReferralInput(ReferralInput input) =>
        input.AssignedQuantity <= 0 || !Precision(input.AssignedQuantity) ||
        Blank(input.ScopeDescription, 2000);

    private static async Task<IResult?> ValidateReferralAssignment(HelpingHandsDbContext database,
        AssistanceRequest request, ReferralInput input, CancellationToken cancellationToken)
    {
        if (request.IsMultiNeed) return Conflict("Multi-need referral is not available yet.");
        if (request.Version != input.Version || request.Status is not ("Accepted" or "Open") || request.RequiredQuantity is null)
            return Conflict("Request changed or is not eligible for referral.");
        if (request.Unit == "scope" && input.AssignedQuantity != 1) return Bad("Service scopes are indivisible.");
        var recipient = await database.Recipients
            .FromSqlInterpolated($"SELECT * FROM \"ReferralRecipient\" WHERE \"Id\" = {input.RecipientId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        if (recipient is null || recipient.RecipientType != "Charity" || recipient.ApprovalStatus != "Approved" || !recipient.Active)
            return Conflict("Recipient is not eligible.");
        var assigned = await database.Referrals.Where(item => item.RequestId == request.Id)
            .SumAsync(item => (decimal?)item.AssignedQuantity, cancellationToken) ?? 0;
        return assigned + input.AssignedQuantity > request.RequiredQuantity
            ? Conflict("Quantity exceeds available need.") : null;
    }
}
