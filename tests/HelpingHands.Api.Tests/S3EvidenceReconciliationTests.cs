using System.Reflection;
using Amazon.S3;
using Amazon.S3.Model;
using HelpingHands.Api.Storage;
using Microsoft.Extensions.Options;

namespace HelpingHands.Api.Tests;

public sealed class S3EvidenceReconciliationTests
{
    [Fact]
    public async Task Stage_WhenInterruptedAfterPartialWrite_CleansObjectAndReconcilesFailedCleanup()
    {
        var client = DispatchProxy.Create<IAmazonS3, S3ClientStub>();
        var stub = (S3ClientStub)client;
        stub.CancelPutAfterWrite = true;
        stub.FailDelete = true;
        var storage = new S3EvidenceStorage(client, Options.Create(new EvidenceStorageOptions { Bucket = "fixture" }));
        using var content = new MemoryStream([1]);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => storage.StageAsync(content, "image/png", CancellationToken.None));
        Assert.Single(stub.Objects);
        Assert.Equal(["PutObjectAsync", "DeleteObjectAsync"], stub.Calls);

        stub.FailDelete = false;
        stub.Objects[0].LastModified = DateTime.UtcNow.AddDays(-2);
        Assert.Equal(1, await storage.CleanupStagedBeforeAsync(DateTimeOffset.UtcNow.AddDays(-1), CancellationToken.None));
        Assert.Empty(stub.Objects);
    }

    [Fact]
    public async Task CleanupOrphanedFinalBefore_DeletesOnlyOldUnreferencedPrivateObjects()
    {
        var client = DispatchProxy.Create<IAmazonS3, S3ClientStub>();
        var stub = (S3ClientStub)client;
        stub.Objects.Add(new S3Object { Key = "private/orphan", LastModified = DateTime.UtcNow.AddDays(-2) });
        stub.Objects.Add(new S3Object { Key = "private/retained", LastModified = DateTime.UtcNow.AddDays(-2) });
        stub.Objects.Add(new S3Object { Key = "private/recent", LastModified = DateTime.UtcNow });
        stub.Objects.Add(new S3Object { Key = "staging/old", LastModified = DateTime.UtcNow.AddDays(-2) });
        var storage = new S3EvidenceStorage(client, Options.Create(new EvidenceStorageOptions { Bucket = "fixture" }));

        var deleted = await storage.CleanupOrphanedFinalBeforeAsync(
            new HashSet<string>(["private/retained"], StringComparer.Ordinal),
            DateTimeOffset.UtcNow.AddDays(-1), CancellationToken.None);

        Assert.Equal(1, deleted);
        Assert.Equal(["private/recent", "private/retained", "staging/old"], stub.Objects.Select(item => item.Key).OrderBy(key => key, StringComparer.Ordinal));
    }
}
