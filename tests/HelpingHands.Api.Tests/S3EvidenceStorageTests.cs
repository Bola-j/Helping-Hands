using System.Reflection;
using Amazon.S3;
using HelpingHands.Api.Storage;
using Microsoft.Extensions.Options;
using Xunit;

namespace HelpingHands.Api.Tests;

public sealed class S3EvidenceStorageTests
{
    [Fact]
    public async Task Stage_SendsUnchunkedUploadToConfiguredBucket()
    {
        var client = DispatchProxy.Create<IAmazonS3, S3ClientStub>();
        var stub = (S3ClientStub)client;
        var storage = new S3EvidenceStorage(client, Options.Create(new EvidenceStorageOptions { Bucket = "fixture" }));
        using var content = new MemoryStream([1, 2, 3]);
        var key = await storage.StageAsync(content, "image/png", CancellationToken.None);
        Assert.StartsWith("staging/", key);
        Assert.Equal("fixture", stub.Upload!.BucketName);
        Assert.False(stub.Upload.UseChunkEncoding);
        Assert.Equal("image/png", stub.Upload.ContentType);
    }

    [Fact]
    public async Task Stage_WhenUploadFails_AttemptsCleanupAndRethrows()
    {
        var client = DispatchProxy.Create<IAmazonS3, S3ClientStub>();
        var stub = (S3ClientStub)client;
        stub.FailPut = true;
        var storage = new S3EvidenceStorage(client, Options.Create(new EvidenceStorageOptions { Bucket = "fixture" }));
        using var content = new MemoryStream([1]);
        await Assert.ThrowsAsync<IOException>(() => storage.StageAsync(content, "image/png", CancellationToken.None));
        Assert.Equal(["PutObjectAsync", "DeleteObjectAsync"], stub.Calls);
    }

    [Fact]
    public async Task Promote_WhenCopyFails_AttemptsBothCleanupsAndRethrows()
    {
        var client = DispatchProxy.Create<IAmazonS3, S3ClientStub>();
        var stub = (S3ClientStub)client;
        stub.FailCopy = true;
        var storage = new S3EvidenceStorage(client, Options.Create(new EvidenceStorageOptions { Bucket = "fixture" }));
        await Assert.ThrowsAsync<IOException>(() => storage.PromoteAsync("staging/fixture", CancellationToken.None));
        Assert.Equal(["CopyObjectAsync", "DeleteObjectAsync", "DeleteObjectAsync"], stub.Calls);
    }

}
