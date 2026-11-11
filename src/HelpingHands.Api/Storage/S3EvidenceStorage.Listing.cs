using Amazon.S3.Model;

namespace HelpingHands.Api.Storage;

public sealed partial class S3EvidenceStorage
{
    private Task<ListObjectsV2Response> ListStagedPageAsync(string? token, CancellationToken ct) =>
        client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = _bucket,
            Prefix = StagingPrefix,
            ContinuationToken = token
        }, ct);
}
