using System.Security.Claims;
using HelpingHands.Api.Data;
using HelpingHands.Api.Storage;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> SaveEvidenceAttachmentAsync(Guid id, IFormFile file, string key,
        ClaimsPrincipal user, HelpingHandsDbContext database, IEvidenceStorage storage, CancellationToken cancellationToken)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var outcome = await database.Outcomes.FromSqlInterpolated(
            $"SELECT * FROM \"Outcome\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
        if (outcome is null || outcome.Status is not ("Draft" or "Clarification"))
        {
            await storage.DeleteAsync(key, CancellationToken.None);
            return Conflict("Outcome is not editable.");
        }

        var attachment = CreateEvidenceAttachment(id, file, key, Actor(user));
        database.EvidenceAttachments.Add(attachment);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Created($"/api/v1/attachments/{attachment.Id}/content",
            new { attachment.Id, attachment.OriginalName, attachment.MediaType, attachment.SizeBytes });
    }

    private static EvidenceAttachment CreateEvidenceAttachment(Guid outcomeId, IFormFile file, string key, Guid actorId) => new()
    {
        OutcomeId = outcomeId,
        StorageKey = key,
        OriginalName = Path.GetFileName(file.FileName),
        MediaType = file.ContentType,
        SizeBytes = file.Length,
        UploadedByUserId = actorId
    };
}
