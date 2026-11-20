using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> CreateReferral(Guid id, ReferralInput input, ClaimsPrincipal user, HelpingHandsDbContext database, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        if (!Coordinator(user)) return Results.Forbid();
        if (InvalidReferralInput(input)) return Bad("A positive quantity and scope description are required.");
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var request = await LockRequest(database, id, cancellationToken);
        if (request is null) return Results.NotFound();
        var error = await ValidateReferralAssignment(database, request, input, timeProvider, cancellationToken);
        if (error is not null) return error;
        var referral = await BuildReferral(database, id, input.RecipientId, input, user, cancellationToken);
        RecordReferralAssignment(database, request, referral, user);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Created($"/api/v1/referrals/{referral.Id}", ReferralView(referral));
    }
    
    private static IQueryable<Referral> VisibleReferrals(HelpingHandsDbContext database, ClaimsPrincipal user)
    {
        var query = database.Referrals.AsNoTracking();
        if (Coordinator(user)) return query;
        var userId = Actor(user);
        if (!user.IsInRole("RecipientRepresentative")) return query.Where(_ => false);
        return query.Where(referral => database.RecipientMembers.Any(membership => membership.RecipientId == referral.RecipientId && membership.UserId == userId && membership.IsActive));
    }

    private static async Task<IResult> ListReferrals(string? reference, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        var query = VisibleReferrals(database, user);
        if (!string.IsNullOrWhiteSpace(reference)) query = query.Where(item => item.Reference == reference);
        return Results.Ok((await query.OrderBy(item => item.Id).Take(100).ToListAsync(cancellationToken)).Select(ReferralView));
    }
}
