using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private async Task AssertCleanupAfterSubmissionAsync(HttpClient representative, Guid outcome)
    {
        var before = await PrivateKeysAsync();
        await EvidenceFlowFixture.UploadAsync(representative, outcome, System.Net.HttpStatusCode.Conflict);
        Assert.Equal(before, await PrivateKeysAsync());
    }

    private async Task<string[]> PrivateKeysAsync()
    {
        var client = fixture.Factory.Services.GetRequiredService<IAmazonS3>();
        var bucket = Environment.GetEnvironmentVariable("P1_TEST_S3_BUCKET")!;
        var result = await client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = bucket });
        Assert.False(result.IsTruncated);
        return result.S3Objects.Select(item => item.Key).OrderBy(key => key, StringComparer.Ordinal).ToArray();
    }
}
