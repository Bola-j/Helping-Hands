using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> SubmitOutcome(Guid id, VersionInput input, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var outcome = await database.Outcomes.FromSqlInterpolated($"SELECT * FROM \"Outcome\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
        if (outcome is null) return Results.NotFound();
        var referral = await database.Referrals.SingleAsync(item => item.Id == outcome.ReferralId, cancellationToken);
        if (!await Member(database, referral.RecipientId, Actor(user), cancellationToken)) return Results.NotFound();
        if (outcome.Version != input.Version || outcome.Status is not ("Draft" or "Clarification")) return Conflict("Outcome changed or cannot be submitted.");
        if (!await database.EvidenceAttachments.AnyAsync(item => item.OutcomeId == id, cancellationToken)) return Bad("An evidence attachment is required.");
        outcome.Status = "Submitted";
        outcome.SubmittedAt = DateTimeOffset.UtcNow;
        outcome.Version = Guid.NewGuid();
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Ok(OutcomeView(outcome));
    }

    private static async Task<IResult> ReviewOutcome(Guid id, ReviewInput input, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (!Coordinator(user)) return Results.Forbid();
        if (InvalidOutcomeReview(input)) return Bad("Invalid review decision.");
        var submittedOutcome = await database.Outcomes.AsNoTracking().SingleOrDefaultAsync(outcome => outcome.Id == id, cancellationToken);
        if (submittedOutcome is null) return Results.NotFound();
        var referralInfo = await database.Referrals.AsNoTracking().SingleAsync(assignedReferral => assignedReferral.Id == submittedOutcome.ReferralId, cancellationToken);
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var request = await LockRequest(database, referralInfo.RequestId, cancellationToken);
        if (request is null) return Results.NotFound();
        var referral = await database.Referrals.SingleAsync(assignedReferral => assignedReferral.Id == submittedOutcome.ReferralId, cancellationToken);
        var outcome = await database.Outcomes.SingleAsync(item => item.Id == id, cancellationToken);
        if (CannotReviewOutcome(outcome, request, input)) return Conflict("Outcome or request changed or cannot be reviewed.");
        if (input.Decision == "Accepted" && !await ApplyAcceptedOutcome(database, request, referral, outcome, cancellationToken))
            return Conflict("Accepted quantity exceeds the referral.");
        RecordOutcomeReview(database, outcome, referral.Id, input, user);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Ok(OutcomeView(outcome));
    }
}
