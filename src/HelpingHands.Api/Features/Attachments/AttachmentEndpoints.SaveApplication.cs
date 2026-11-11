using System.Security.Claims;
using HelpingHands.Api.Data;
using HelpingHands.Api.Storage;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> SaveApplicationAttachmentAsync(Guid id, IFormFile file, string key,
        ClaimsPrincipal user, HelpingHandsDbContext database, IEvidenceStorage storage, CancellationToken cancellationToken)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var recipient = await database.Recipients.FromSqlInterpolated(
            $"SELECT * FROM \"ReferralRecipient\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
        if (recipient is null || recipient.ApprovalStatus != "Pending")
        {
            await storage.DeleteAsync(key, CancellationToken.None);
            return Conflict("Application is not pending.");
        }

        var attachment = CreateApplicationAttachment(id, file, key, Actor(user));
        database.ApplicationAttachments.Add(attachment);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Created($"/api/v1/attachments/{attachment.Id}/content",
            new { attachment.Id, attachment.OriginalName, attachment.MediaType, attachment.SizeBytes });
    }

    private static ApplicationAttachment CreateApplicationAttachment(Guid recipientId, IFormFile file, string key, Guid actorId) => new()
    {
        RecipientId = recipientId,
        StorageKey = key,
        OriginalName = Path.GetFileName(file.FileName),
        MediaType = file.ContentType,
        SizeBytes = file.Length,
        UploadedByUserId = actorId
    };
}
