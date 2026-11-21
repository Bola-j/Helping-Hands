using System.Data;
using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> CreateSingleNeedRequest(SingleNeedRequestInput input, ClaimsPrincipal user,
        HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (!Beneficiary(user)) return Results.Forbid();
        if (!input.AuthorityConfirmed || !input.SharingAgreed || Blank(input.Description, 4000))
            return Bad("Description, authority and sharing confirmations are required.");
        var userId = Actor(user);
        if (!await database.Beneficiaries.AnyAsync(item => item.Id == input.BeneficiaryId && item.LinkedUserId == userId, cancellationToken))
            return Results.NotFound();
        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        var request = new AssistanceRequest
        {
            BeneficiaryId = input.BeneficiaryId,
            SubmittedByUserId = userId,
            Description = input.Description.Trim(),
            AuthorityConfirmed = true,
            SharingAgreed = true,
            IsMultiNeed = false,
            Reference = await database.NextReferenceAsync("REQ", cancellationToken)
        };
        database.Requests.Add(request);
        database.DecisionHistory.Add(new DecisionHistory { RequestId = request.Id, ActorUserId = userId, Action = "Submitted" });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Created($"/api/v1/requests/{request.Id}", RequestView(request));
    }
}
