using System.Security.Claims;
using HelpingHands.Api.Data;
using HelpingHands.Api.Storage;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private sealed record AttachmentDownload(string Key, string Name, string MediaType);

    private static async Task<IResult> DownloadAttachment(Guid id, HttpContext context, ClaimsPrincipal user,
        HelpingHandsDbContext database, IEvidenceStorage storage, CancellationToken cancellationToken)
    {
        var applicationAttachment = await database.ApplicationAttachments.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        var details = applicationAttachment is null
            ? await FindEvidenceDownloadAsync(id, user, database, cancellationToken)
            : await FindApplicationDownloadAsync(applicationAttachment, user, database, cancellationToken);
        if (details is null)
            return Results.NotFound();

        var response = await storage.OpenReadAsync(details.Key, cancellationToken);
        context.Response.RegisterForDispose(response);
        context.Response.Headers.CacheControl = "no-store";
        return Results.Stream(response.ResponseStream, details.MediaType, details.Name);
    }
}
