using HelpingHands.Api.Storage;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> StoreAttachmentAsync(IFormFile file, IEvidenceStorage storage,
        Func<string, CancellationToken, Task<IResult>> saveMetadata, CancellationToken cancellationToken)
    {
        string? stagedKey = null;
        string? finalKey = null;
        try
        {
            await using var stream = file.OpenReadStream();
            stagedKey = await storage.StageAsync(stream, file.ContentType, cancellationToken);
            finalKey = await storage.PromoteAsync(stagedKey, cancellationToken);
            stagedKey = null;
            return await saveMetadata(finalKey, cancellationToken);
        }
        catch
        {
            if (stagedKey is not null)
                await TryDeleteAttachmentAsync(storage, stagedKey);
            // Keep promoted objects until reconciliation resolves an ambiguous database commit.
            throw;
        }
    }

    private static async Task TryDeleteAttachmentAsync(IEvidenceStorage storage, string key)
    {
        try { await storage.DeleteAsync(key, CancellationToken.None); }
        catch { /* Preserve the original failure, as cleanup is best-effort. */ }
    }
}
