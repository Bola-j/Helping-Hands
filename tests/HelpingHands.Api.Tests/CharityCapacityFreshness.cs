using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private sealed class MutableCairoTimeProvider(DateTimeOffset initialUtc) : TimeProvider
    {
        private DateTimeOffset _utcNow = initialUtc;
        public override DateTimeOffset GetUtcNow() => _utcNow;
        public void SetUtcNow(DateTimeOffset utcNow) => _utcNow = utcNow;
    }

    private static HttpClient CreateHttpsClient(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

    [Fact]
    public async Task DailyCapacityFreshness_DistinguishesStaleFromZeroAndFreezesAtCairoMidnight()
    {
        // 2026-11-20 23:58:00 Cairo (UTC+2) == 2026-11-20 21:58:00 UTC
        var beforeMidnightUtc = new DateTimeOffset(2026, 11, 20, 21, 58, 0, TimeSpan.Zero);
        var timeProvider = new MutableCairoTimeProvider(beforeMidnightUtc);

        await using var customFactory = fixture.Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(timeProvider);
            });
        });

        using var scope = customFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var (_, service1, _, version1) = await PublishedCatalogAsync(db);
        var (_, service2, _, version2) = await PublishedCatalogAsync(db);

        // Register charity representative on customFactory
        var repClient = CreateHttpsClient(customFactory);
        var repEmail = $"rep-fresh-{Guid.NewGuid():N}@example.test";
        var regRep = await WorkflowDbFixture.RegisterCharityAsync(
            repClient, "Rep Freshness", repEmail, fixture.Password, "Freshness Charity", "fresh@example.test", "REG-P121");
        Assert.Equal(HttpStatusCode.Created, regRep.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await WorkflowDbFixture.LoginRegisteredAsync(repClient, repEmail, fixture.Password)).StatusCode);

        var charity = await WorkflowDbFixture.RecipientAsync(repClient);
        var recipientId = charity.GetProperty("id").GetGuid();

        // Approve charity as Administrator
        var adminClient = CreateHttpsClient(customFactory);
        Assert.Equal(HttpStatusCode.NoContent,
            (await WorkflowDbFixture.LoginRegisteredAsync(adminClient, fixture.Emails["Administrator"], fixture.Password)).StatusCode);
        await WorkflowDbFixture.ApproveAsync(adminClient, charity);

        // Login as Coordinator
        var coordinatorClient = CreateHttpsClient(customFactory);
        Assert.Equal(HttpStatusCode.NoContent,
            (await WorkflowDbFixture.LoginRegisteredAsync(coordinatorClient, fixture.Emails["Coordinator"], fixture.Password)).StatusCode);

        // Select two services
        using var selectRes = await PutWithAntiforgeryAsync(repClient, $"/api/v1/recipients/{recipientId}/services", new
        {
            Services = new[]
            {
                new { ServiceVersionId = service1.Id, StandardVersionId = version1.Id },
                new { ServiceVersionId = service2.Id, StandardVersionId = version2.Id }
            }
        });
        var selectBody = await WorkflowDbFixture.JsonAsync(selectRes, HttpStatusCode.OK);
        var selected = selectBody.GetProperty("selectedServices").EnumerateArray().ToList();
        var cs1Id = selected.Single(x => x.GetProperty("serviceVersionId").GetGuid() == service1.Id).GetProperty("charityServiceId").GetGuid();
        var cs2Id = selected.Single(x => x.GetProperty("serviceVersionId").GetGuid() == service2.Id).GetProperty("charityServiceId").GetGuid();

        // 1. Before any update today -> both are Stale (distinct from Zero)
        using var initialCapGet = await repClient.GetAsync($"/api/v1/recipients/{recipientId}/capacity");
        var initialCapBody = await WorkflowDbFixture.JsonAsync(initialCapGet, HttpStatusCode.OK);
        var initialItems = initialCapBody.GetProperty("capacities").EnumerateArray().ToList();
        Assert.All(initialItems, item =>
        {
            Assert.Equal("Stale", item.GetProperty("status").GetString());
            Assert.True(item.GetProperty("isStale").GetBoolean());
            Assert.Equal(JsonValueKind.Null, item.GetProperty("available").ValueKind);
        });

        // 2. Update service1 to 0 (Zero) and leave service2 un-updated (Stale)
        using var updateZeroRes = await PutWithAntiforgeryAsync(repClient, $"/api/v1/recipients/{recipientId}/capacity", new
        {
            Capacities = new[] { new { CharityServiceId = cs1Id, Available = 0m } }
        });
        var afterZeroBody = await WorkflowDbFixture.JsonAsync(updateZeroRes, HttpStatusCode.OK);
        var itemsAfterZero = afterZeroBody.GetProperty("capacities").EnumerateArray().ToList();
        var item1 = itemsAfterZero.Single(x => x.GetProperty("charityServiceId").GetGuid() == cs1Id);
        var item2 = itemsAfterZero.Single(x => x.GetProperty("charityServiceId").GetGuid() == cs2Id);

        Assert.Equal("Zero", item1.GetProperty("status").GetString());
        Assert.False(item1.GetProperty("isStale").GetBoolean());
        Assert.Equal(0m, item1.GetProperty("available").GetDecimal());

        Assert.Equal("Stale", item2.GetProperty("status").GetString());
        Assert.True(item2.GetProperty("isStale").GetBoolean());
        Assert.Equal(JsonValueKind.Null, item2.GetProperty("available").ValueKind);

        // 3. Create an accepted request & verify referral succeeds for fresh service1, but is blocked for stale service2
        var benClient = CreateHttpsClient(customFactory);
        var benEmail = $"ben-fresh-{Guid.NewGuid():N}@example.test";
        Assert.Equal(HttpStatusCode.Created, (await WorkflowDbFixture.RegisterBeneficiaryAsync(benClient, "Ben Fresh", benEmail, fixture.Password)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await WorkflowDbFixture.LoginRegisteredAsync(benClient, benEmail, fixture.Password)).StatusCode);

        var profile = await WorkflowDbFixture.BeneficiaryAsync(benClient);
        var request = await WorkflowDbFixture.RequestAsync(benClient, profile.GetProperty("id").GetGuid());
        var requestId = request.GetProperty("id").GetGuid();
        var reqVersion = request.GetProperty("version").GetGuid();

        using var acceptReqRes = await PostWithAntiforgeryAsync(coordinatorClient, $"/api/v1/requests/{requestId}/decisions", new
        {
            Decision = "Accepted",
            Category = "Goods",
            ServiceSubtype = service1.Name,
            RequiredQuantity = 5m,
            Unit = "boxes",
            Version = reqVersion
        });
        var acceptedReq = await WorkflowDbFixture.JsonAsync(acceptReqRes, HttpStatusCode.OK);
        var acceptedVersion = acceptedReq.GetProperty("version").GetGuid();

        // Blocked for stale service2
        using var staleRefRes = await PostWithAntiforgeryAsync(coordinatorClient, $"/api/v1/requests/{requestId}/referrals", new
        {
            RecipientId = recipientId,
            AssignedQuantity = 2m,
            ScopeDescription = "Assign stale service2",
            Version = acceptedVersion,
            ServiceVersionId = service2.Id,
            StandardVersionId = version2.Id
        });
        Assert.Equal(HttpStatusCode.Conflict, staleRefRes.StatusCode);

        // Allowed for fresh service1 before midnight
        using var freshRefRes = await PostWithAntiforgeryAsync(coordinatorClient, $"/api/v1/requests/{requestId}/referrals", new
        {
            RecipientId = recipientId,
            AssignedQuantity = 2m,
            ScopeDescription = "Assign fresh service1 before Cairo midnight",
            Version = acceptedVersion,
            ServiceVersionId = service1.Id,
            StandardVersionId = version1.Id
        });
        var createdReferral = await WorkflowDbFixture.JsonAsync(freshRefRes, HttpStatusCode.Created);
        var existingReferralId = createdReferral.GetProperty("id").GetGuid();

        // Refresh request version after first referral
        using var getReqAfterFirst = await coordinatorClient.GetAsync($"/api/v1/requests/{requestId}");
        var reqAfterFirstBody = await WorkflowDbFixture.JsonAsync(getReqAfterFirst, HttpStatusCode.OK);
        var versionAfterFirstRef = reqAfterFirstBody.GetProperty("version").GetGuid();

        // 4. Advance clock past Cairo midnight -> 2026-11-21 00:02:00 Cairo (2026-11-20 22:02:00 UTC)
        timeProvider.SetUtcNow(new DateTimeOffset(2026, 11, 20, 22, 2, 0, TimeSpan.Zero));

        // Service1 becomes Stale automatically at Cairo midnight
        using var afterMidnightCapGet = await repClient.GetAsync($"/api/v1/recipients/{recipientId}/capacity");
        var afterMidnightBody = await WorkflowDbFixture.JsonAsync(afterMidnightCapGet, HttpStatusCode.OK);
        Assert.Equal("2026-11-21", afterMidnightBody.GetProperty("cairoDate").GetString());
        var item1AfterMidnight = afterMidnightBody.GetProperty("capacities").EnumerateArray()
            .Single(x => x.GetProperty("charityServiceId").GetGuid() == cs1Id);
        Assert.Equal("Stale", item1AfterMidnight.GetProperty("status").GetString());
        Assert.True(item1AfterMidnight.GetProperty("isStale").GetBoolean());

        // New assignment to service1 is now frozen/blocked
        using var frozenRefRes = await PostWithAntiforgeryAsync(coordinatorClient, $"/api/v1/requests/{requestId}/referrals", new
        {
            RecipientId = recipientId,
            AssignedQuantity = 1m,
            ScopeDescription = "Assign service1 after Cairo midnight",
            Version = versionAfterFirstRef,
            ServiceVersionId = service1.Id,
            StandardVersionId = version1.Id
        });
        Assert.Equal(HttpStatusCode.Conflict, frozenRefRes.StatusCode);

        // Existing referral created before midnight is preserved intact
        using var existingRefGet = await coordinatorClient.GetAsync($"/api/v1/referrals/{existingReferralId}");
        var existingRefBody = await WorkflowDbFixture.JsonAsync(existingRefGet, HttpStatusCode.OK);
        Assert.Equal("Assigned", existingRefBody.GetProperty("status").GetString());
        Assert.Equal(2m, existingRefBody.GetProperty("assignedQuantity").GetDecimal());
    }

    private static async Task<HttpResponseMessage> PostWithAntiforgeryAsync(HttpClient client, string path, object body)
    {
        using var tokenResponse = await client.GetAsync("/api/v1/auth/antiforgery");
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return await client.SendAsync(request);
    }
}