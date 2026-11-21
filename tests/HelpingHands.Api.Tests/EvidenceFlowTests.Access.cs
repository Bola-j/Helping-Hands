using System.Net;
using HelpingHands.Api.Data;
using HelpingHands.Api.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private async Task AssertAttachmentAccessAsync(HttpClient owner, HttpClient outsider, Guid attachment)
    {
        var path = $"/api/v1/attachments/{attachment}/content";
        using var allowed = await owner.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        Assert.Equal("%PDF-1.4\nfictional evidence\n%%EOF", await allowed.Content.ReadAsStringAsync());
        var storage = (EvidenceStorageRecordingProxy)(object)fixture.Factory.Services.GetRequiredService<IEvidenceStorage>();
        var readsAfterOwner = storage.OpenReadCalls;
        using var denied = await outsider.GetAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode);
        Assert.Equal(readsAfterOwner, storage.OpenReadCalls);
        using var anonymous = fixture.Client();
        using var anonymousResult = await anonymous.GetAsync(path);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResult.StatusCode);
        Assert.Equal(readsAfterOwner, storage.OpenReadCalls);
        await AssertObjectPrivateAsync(attachment);
    }

    private async Task AssertObjectPrivateAsync(Guid attachment)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var row = await db.EvidenceAttachments.SingleAsync(item => item.Id == attachment);
        var bucket = Environment.GetEnvironmentVariable("P1_TEST_S3_BUCKET")!;
        var service = Environment.GetEnvironmentVariable("P1_TEST_S3_URL")!.TrimEnd('/');
        using var client = new HttpClient();
        using var response = await client.GetAsync($"{service}/{bucket}/{row.StorageKey}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
