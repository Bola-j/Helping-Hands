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
    public async Task RequestPaginationReturnsAllAuthorizedRowsAndRejectsInvalidCursors()
    {
        using var client = fixture.Client();
        var email = $"paging-{Guid.NewGuid():N}@example.test";
        using var registration = await WorkflowDbFixture.RegisterBeneficiaryAsync(client, "Fictional", email, fixture.Password);
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
        using var login = await WorkflowDbFixture.LoginRegisteredAsync(client, email, fixture.Password);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        var (ids, reference) = await SeedPaginationRequestsAsync(email);
        using var page1 = await client.GetAsync("/api/v1/requests?limit=2");
        var first = await page1.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, first.GetArrayLength());
        var cursor = Assert.Single(page1.Headers.GetValues("X-Next-Cursor"));
        using var page2 = await client.GetAsync($"/api/v1/requests?limit=2&cursor={cursor}");
        var second = await page2.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(second.EnumerateArray());
        Assert.False(page2.Headers.Contains("X-Next-Cursor"));
        Assert.True(ids.SetEquals(first.EnumerateArray().Concat(second.EnumerateArray()).Select(x => x.GetProperty("id").GetGuid())));
        using var exact = await client.GetAsync($"/api/v1/requests?reference={reference}");
        Assert.Single((await exact.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray());
        using var unrelated = await fixture.LoginAsync("Unrelated");
        using var hidden = await unrelated.GetAsync($"/api/v1/requests?reference={reference}");
        Assert.Empty((await hidden.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray());
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/v1/requests?cursor=invalid")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/v1/requests?limit=101")).StatusCode);
    }

    private async Task<(HashSet<Guid> Ids, string Reference)> SeedPaginationRequestsAsync(string email)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var user = await db.Users.SingleAsync(x => x.Email == email);
        var profile = await db.Beneficiaries.SingleAsync(x => x.LinkedUserId == user.Id);
        var requests = new List<AssistanceRequest>();
        for (var i = 0; i < 3; i++)
            requests.Add(new AssistanceRequest { BeneficiaryId = profile.Id, SubmittedByUserId = user.Id, Reference = await db.NextReferenceAsync("REQ"), Description = "Fictional", AuthorityConfirmed = true, SharingAgreed = true });
        db.Requests.AddRange(requests);
        await db.SaveChangesAsync();
        return (requests.Select(x => x.Id).ToHashSet(), requests[0].Reference);
    }
}
