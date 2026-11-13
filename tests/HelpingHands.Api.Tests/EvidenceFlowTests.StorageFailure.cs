using Amazon.Runtime;
using Amazon.S3;
using HelpingHands.Api.Storage;
using Microsoft.Extensions.Options;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    [Fact]
    public async Task StorageFailure_WithInvalidCredentials_DoesNotCreateAnObject()
    {
        var before = await PrivateKeysAsync();
        var config = new AmazonS3Config
        {
            ServiceURL = Environment.GetEnvironmentVariable("P1_TEST_S3_URL"),
            ForcePathStyle = true, AuthenticationRegion = "us-east-1"
        };
        using var client = new AmazonS3Client(new BasicAWSCredentials(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N")), config);
        var storage = new S3EvidenceStorage(client, Options.Create(new EvidenceStorageOptions
        {
            Bucket = Environment.GetEnvironmentVariable("P1_TEST_S3_BUCKET")!
        }));
        using var content = new MemoryStream("fictional evidence"u8.ToArray());
        await Assert.ThrowsAnyAsync<AmazonS3Exception>(() => storage.StageAsync(content, "application/pdf", CancellationToken.None));
        Assert.Equal(before, await PrivateKeysAsync());
    }
}
