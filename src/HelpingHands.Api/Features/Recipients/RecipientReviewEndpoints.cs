using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> ListRecipients(string? reference, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        var userId = Actor(user);
        var query = database.Recipients.AsNoTracking().AsQueryable();
        if (!Staff(user)) query = query.Where(recipient => database.RecipientMembers.Any(membership => membership.UserId == userId && membership.RecipientId == recipient.Id && membership.IsActive));
        if (!string.IsNullOrWhiteSpace(reference)) query = query.Where(recipient => recipient.Reference == reference);
        return Results.Ok((await query.OrderBy(item => item.Id).Take(100).ToListAsync(cancellationToken)).Select(RecipientView));
    }

    private static async Task<IResult> GetRecipient(Guid id, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        var recipient = await database.Recipients.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (recipient is null || (!Staff(user) && !await Member(database, id, Actor(user), cancellationToken))) return Results.NotFound();
        var charity = await database.Charities.AsNoTracking().SingleAsync(item => item.RecipientId == id, cancellationToken);
        return Results.Ok(new { recipient.Id, recipient.Reference, recipient.DisplayName, recipient.ContactDetails, recipient.ApprovalStatus, recipient.Active, recipient.Version, charity.RegistrationInformation });
    }

    private static async Task<IResult> DecideRecipient(Guid id, RecipientDecisionInput input, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (!Coordinator(user)) return Results.Forbid();
        if (input.Decision is not ("Approved" or "Rejected") || (input.Decision == "Rejected" && Blank(input.Reason, 2000)) || input.Reason?.Length > 2000) return Bad("Choose approval or rejection; rejection requires a reason.");
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var recipient = await database.Recipients.FromSqlInterpolated($"SELECT * FROM \"ReferralRecipient\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
        if (recipient is null) return Results.NotFound();
        if (recipient.Version != input.Version || recipient.ApprovalStatus != "Pending") return Conflict("Application changed or is no longer pending.");
        var charity = await database.Charities.SingleAsync(item => item.RecipientId == id, cancellationToken);
        if (input.Decision == "Approved" && string.IsNullOrWhiteSpace(charity.RegistrationInformation) && !await database.ApplicationAttachments.AnyAsync(item => item.RecipientId == id, cancellationToken)) return Bad("Registration information or an attachment is required.");
        recipient.ApprovalStatus = input.Decision;
        recipient.Active = input.Decision == "Approved";
        recipient.Version = Guid.NewGuid();
        database.RecipientDecisions.Add(new RecipientDecision { RecipientId = id, ActorUserId = Actor(user), Decision = input.Decision, Reason = input.Reason });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Ok(RecipientView(recipient));
    }
}
