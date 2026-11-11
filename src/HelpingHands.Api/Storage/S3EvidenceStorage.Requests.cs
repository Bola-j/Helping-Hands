using Amazon.S3.Model;

namespace HelpingHands.Api.Storage;

public sealed partial class S3EvidenceStorage
{
    private PutObjectRequest CreateUpload(string key, Stream content, string mediaType) => new()
    {
        BucketName = _bucket,
        Key = key,
        InputStream = content,
        ContentType = mediaType,
        AutoCloseStream = false,
        UseChunkEncoding = false
    };

    private CopyObjectRequest CreateCopy(string stagedKey, string finalKey) => new()
    {
        SourceBucket = _bucket,
        SourceKey = stagedKey,
        DestinationBucket = _bucket,
        DestinationKey = finalKey
    };
}
