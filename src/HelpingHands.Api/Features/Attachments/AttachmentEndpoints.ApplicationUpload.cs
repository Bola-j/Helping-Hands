using System.Security.Claims;
using HelpingHands.Api.Data;
using HelpingHands.Api.Storage;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> UploadApplication(Guid id, HttpRequest request, ClaimsPrincipal user,
        HelpingHandsDbContext database, IEvidenceStorage storage, CancellationToken cancellationToken)
    {
        if (!await Member(database, id, Actor(user), cancellationToken))
            return Results.NotFound();

        var file = await ReadValidAttachmentAsync(request, cancellationToken);
        if (file is null)
            return Bad("Provide one PDF, JPEG or PNG of up to 10 MiB.");

        return await StoreAttachmentAsync(file, storage,
            (key, token) => SaveApplicationAttachmentAsync(id, file, key, user, database, storage, token),
            cancellationToken);
    }
}
