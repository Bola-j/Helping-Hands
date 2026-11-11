namespace HelpingHands.Api.Storage;

public sealed partial class S3EvidenceStorage
{
    private async Task TryDeleteAsync(string key)
    {
        try
        {
            await DeleteAsync(key, CancellationToken.None);
        }
        catch
        {
            // Cleanup is best-effort; preserve the original storage failure.
        }
    }
}
