using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<AttachmentDownload?> FindApplicationDownloadAsync(ApplicationAttachment attachment,
        ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (!Staff(user) && !await Member(database, attachment.RecipientId, Actor(user), cancellationToken))
            return null;
        return new AttachmentDownload(attachment.StorageKey, attachment.OriginalName, attachment.MediaType);
    }

    private static async Task<AttachmentDownload?> FindEvidenceDownloadAsync(Guid attachmentId, ClaimsPrincipal user,
        HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        var attachment = await database.EvidenceAttachments.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == attachmentId, cancellationToken);
        if (attachment is null || !await VisibleOutcomes(database, user).AnyAsync(x => x.Id == attachment.OutcomeId, cancellationToken))
            return null;
        return new AttachmentDownload(attachment.StorageKey, attachment.OriginalName, attachment.MediaType);
    }
}
