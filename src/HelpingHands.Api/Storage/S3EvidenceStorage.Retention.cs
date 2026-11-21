using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
namespace HelpingHands.Api.Storage;

public sealed partial class S3EvidenceStorage
{
    public Task DeleteAsync(string key, CancellationToken cancellationToken) =>
            client.DeleteObjectAsync(_bucket, key, cancellationToken);

    public async Task<int> CleanupStagedBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken)
    {
        var count = 0;
        string? token = null;
        do
        {
            var response = await ListStagedPageAsync(token, cancellationToken);
            foreach (var item in response.S3Objects)
            {
                if (item.LastModified.ToUniversalTime() >= cutoff.UtcDateTime)
                    continue;
                await DeleteAsync(item.Key, cancellationToken);
                count++;
            }
            token = response.IsTruncated ? response.NextContinuationToken : null;
        } while (token is not null);
        return count;
    }

    public async Task<int> CleanupOrphanedFinalBeforeAsync(IReadOnlySet<string> retainedKeys, DateTimeOffset cutoff, CancellationToken cancellationToken)
    {
        var count = 0;
        string? token = null;
        do
        {
            var response = await ListPrivatePageAsync(token, cancellationToken);
            foreach (var item in response.S3Objects)
            {
                if (item.LastModified.ToUniversalTime() >= cutoff.UtcDateTime || retainedKeys.Contains(item.Key))
                    continue;
                await DeleteAsync(item.Key, cancellationToken);
                count++;
            }
            token = response.IsTruncated ? response.NextContinuationToken : null;
        } while (token is not null);
        return count;
    }
}
