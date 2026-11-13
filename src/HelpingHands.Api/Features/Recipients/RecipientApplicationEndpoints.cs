using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static object OutcomeView(Outcome outcome) => new { outcome.Id, outcome.Reference, outcome.ReferralId, outcome.ReportedQuantity, outcome.Description, outcome.Status, outcome.SubmittedAt, outcome.Version };

    public static RouteGroupBuilder MapDemoEndpoints(this RouteGroupBuilder api)
    {
        api.MapParticipantRoutes();
        api.MapRequestAndReferralRoutes();
        api.MapOutcomeRoutes();
        api.MapAttachmentEndpoints();
        return api;
    }

    private static async Task<IResult> CreateRecipient(RecipientInput input, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (!user.IsInRole("RecipientRepresentative")) return Results.Forbid();
        if (Blank(input.DisplayName, 200) || Blank(input.ContactDetails, 500) || input.RegistrationInformation?.Length > 1000) return Bad("Valid name, contact and registration information are required.");
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var recipient = new Recipient { Reference = await database.NextReferenceAsync("REC", cancellationToken), DisplayName = input.DisplayName.Trim(), ContactDetails = input.ContactDetails.Trim() };
        database.Recipients.Add(recipient);
        database.Charities.Add(new Charity { RecipientId = recipient.Id, RegistrationInformation = input.RegistrationInformation?.Trim() });
        database.RecipientMembers.Add(new RecipientMember { RecipientId = recipient.Id, UserId = Actor(user) });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Created($"/api/v1/recipients/{recipient.Id}", RecipientView(recipient));
    }
}
