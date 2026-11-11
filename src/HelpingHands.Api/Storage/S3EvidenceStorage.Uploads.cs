using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
namespace HelpingHands.Api.Storage;

public sealed partial class S3EvidenceStorage(IAmazonS3 client, IOptions<EvidenceStorageOptions> options) : IEvidenceStorage
{
    private readonly string _bucket = options.Value.Bucket;

    private const string StagingPrefix = "staging/";

    private const string FinalPrefix = "private/";

    public async Task<string> StageAsync(Stream content, string mediaType, CancellationToken cancellationToken)
    {
        var key = StagingPrefix + Guid.NewGuid().ToString("N");
        try
        {
            await client.PutObjectAsync(CreateUpload(key, content, mediaType), cancellationToken);
            return key;
        }
        catch
        {
            // An unsuccessful upload may still have created an object.
            await TryDeleteAsync(key);
            throw;
        }
    }

    public async Task<string> PromoteAsync(string stagedKey, CancellationToken cancellationToken)
    {
        if (!stagedKey.StartsWith(StagingPrefix, StringComparison.Ordinal))
            throw new ArgumentException("Expected a staged object key.", nameof(stagedKey));
        var finalKey = FinalPrefix + Guid.NewGuid().ToString("N");
        try
        {
            await client.CopyObjectAsync(CreateCopy(stagedKey, finalKey), cancellationToken);
            await DeleteAsync(stagedKey, cancellationToken);
            return finalKey;
        }
        catch
        {
            await TryDeleteAsync(finalKey);
            await TryDeleteAsync(stagedKey);
            throw;
        }
    }

    public Task<GetObjectResponse> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        if (!key.StartsWith(FinalPrefix, StringComparison.Ordinal))
            throw new ArgumentException("Expected a finalized object key.", nameof(key));
        return client.GetObjectAsync(_bucket, key, cancellationToken);
    }
}
