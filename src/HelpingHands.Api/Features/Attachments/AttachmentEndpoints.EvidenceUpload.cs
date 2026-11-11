using System.Security.Claims;
using HelpingHands.Api.Data;
using HelpingHands.Api.Storage;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> UploadEvidence(Guid id, HttpRequest request, ClaimsPrincipal user,
        HelpingHandsDbContext database, IEvidenceStorage storage, CancellationToken cancellationToken)
    {
        var outcome = await database.Outcomes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (outcome is null)
            return Results.NotFound();
        var referral = await database.Referrals.AsNoTracking().SingleAsync(x => x.Id == outcome.ReferralId, cancellationToken);
        if (!await Member(database, referral.RecipientId, Actor(user), cancellationToken))
            return Results.NotFound();

        var file = await ReadValidAttachmentAsync(request, cancellationToken);
        if (file is null)
            return Bad("Provide one PDF, JPEG or PNG of up to 10 MiB.");

        return await StoreAttachmentAsync(file, storage,
            (key, token) => SaveEvidenceAttachmentAsync(id, file, key, user, database, storage, token), cancellationToken);
    }
}
