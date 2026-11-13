using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<object> RecipientDetailAsync(HelpingHandsDbContext database, Recipient recipient,
        Charity charity, CancellationToken cancellationToken)
    {
        var attachments = await database.ApplicationAttachments.AsNoTracking()
            .Where(item => item.RecipientId == recipient.Id).OrderBy(item => item.UploadedAt).ThenBy(item => item.Id)
            .Select(item => new { item.Id, item.OriginalName, item.MediaType, item.SizeBytes, item.UploadedAt })
            .ToListAsync(cancellationToken);
        var decisions = await database.RecipientDecisions.AsNoTracking()
            .Where(item => item.RecipientId == recipient.Id).OrderBy(item => item.DecidedAt).ThenBy(item => item.Id)
            .Select(item => new { item.Decision, item.Reason, item.ActorUserId, item.DecidedAt })
            .ToListAsync(cancellationToken);
        return new { recipient.Id, recipient.Reference, recipient.DisplayName, recipient.ContactDetails,
            recipient.ApprovalStatus, recipient.Active, recipient.Version, charity.RegistrationInformation, attachments, decisions };
    }
}
