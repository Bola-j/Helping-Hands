using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task MultiNeedSubmissionRecordsCurrentVersionsAndReadsHistoricalContents()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, profile) = await LinkedProfileAsync(scope);
        var (_, _, standard, first) = await PublishedCatalogAsync(db);
        var (_, _, _, other) = await PublishedCatalogAsync(db);
        using var client = await fixture.LoginAsync("Beneficiary");
        using var response = await PostNeedsAsync(client, profile.Id, new[] { new { StandardVersionId = first.Id, Quantity = 2m }, new { StandardVersionId = other.Id, Quantity = 3m } });
        var request = await WorkflowDbFixture.JsonAsync(response, HttpStatusCode.Created);
        Assert.StartsWith("REQ-", request.GetProperty("reference").GetString());
        Assert.Equal(2, request.GetProperty("needs").GetArrayLength());
        var next = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 2, EffectiveAt = DateTimeOffset.UtcNow.AddMinutes(-1) };
        await PublishAsync(db, next);
        using var read = await client.GetAsync($"/api/v1/requests/{request.GetProperty("id").GetGuid()}");
        var saved = await read.Content.ReadFromJsonAsync<JsonElement>();
        var original = saved.GetProperty("needs").EnumerateArray().Single(x => x.GetProperty("standardVersionId").GetGuid() == first.Id);
        Assert.Equal("Rice", original.GetProperty("components")[0].GetProperty("name").GetString());
        Assert.Equal(1, original.GetProperty("standardVersionNumber").GetInt32());
        using var unrelated = await fixture.LoginAsync("Unrelated");
        Assert.Equal(HttpStatusCode.NotFound, (await unrelated.GetAsync($"/api/v1/requests/{request.GetProperty("id").GetGuid()}")).StatusCode);
    }

    [Fact]
    public async Task MultiNeedSubmissionRejectsInvalidStaleAndUnlinkedProfilesAtomically()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, profile) = await LinkedProfileAsync(scope);
        var (_, _, standard, first) = await PublishedCatalogAsync(db);
        var second = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 2, EffectiveAt = DateTimeOffset.UtcNow.AddMinutes(-1) };
        await PublishAsync(db, second);
        using var client = await fixture.LoginAsync("Beneficiary");
        var before = await db.Requests.CountAsync(x => x.BeneficiaryId == profile.Id);
        using var stale = await PostNeedsAsync(client, profile.Id, new[] { new { StandardVersionId = first.Id, Quantity = 1m } });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        using var duplicate = await PostNeedsAsync(client, profile.Id, new[] { new { StandardVersionId = second.Id, Quantity = 1m }, new { StandardVersionId = second.Id, Quantity = 1m } });
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        using var zero = await PostNeedsAsync(client, profile.Id, new[] { new { StandardVersionId = second.Id, Quantity = 0m } });
        Assert.Equal(HttpStatusCode.BadRequest, zero.StatusCode);
        var unlinked = new Beneficiary { CreatedByUserId = fixture.UserIds["Beneficiary"], Reference = await db.NextReferenceAsync("BEN"), DisplayName = "Fictional" };
        db.Beneficiaries.Add(unlinked);
        await db.SaveChangesAsync();
        using var denied = await PostNeedsAsync(client, unlinked.Id, new[] { new { StandardVersionId = second.Id, Quantity = 1m } });
        Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode);
        Assert.Equal(before, await db.Requests.CountAsync(x => x.BeneficiaryId == profile.Id));
    }

    private static Task<HttpResponseMessage> PostNeedsAsync(HttpClient client, Guid beneficiaryId, object needs) =>
        WorkflowDbFixture.PostAsync(client, "/api/v1/requests", new { BeneficiaryId = beneficiaryId, Description = "Fictional request", AuthorityConfirmed = true, SharingAgreed = true, Needs = needs });
}
