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
    public async Task CharityServiceSelection_AllowsRepresentativeToViewAndUpdateSupportedServices()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var (category1, service1, standard1, version1) = await PublishedCatalogAsync(db);
        var (_, service2, _, version2) = await PublishedCatalogAsync(db);

        var repClient = await CreateLoggedInCharityRepresentativeAsync("Rep Alpha");
        var charity = await WorkflowDbFixture.RecipientAsync(repClient);
        var recipientId = charity.GetProperty("id").GetGuid();

        // 1. View initial state & applicable platform-unit versions
        using var initialGet = await repClient.GetAsync($"/api/v1/recipients/{recipientId}/services");
        var initialBody = await WorkflowDbFixture.JsonAsync(initialGet, HttpStatusCode.OK);
        Assert.Empty(initialBody.GetProperty("selectedServices").EnumerateArray());
        var options = initialBody.GetProperty("applicableOptions").EnumerateArray().ToList();
        Assert.Contains(options, o =>
            o.GetProperty("categoryId").GetGuid() == category1.Id &&
            o.GetProperty("serviceVersionId").GetGuid() == service1.Id &&
            o.GetProperty("unitStandardId").GetGuid() == standard1.Id &&
            o.GetProperty("standardVersionId").GetGuid() == version1.Id);
        var option = options.Single(o => o.GetProperty("standardVersionId").GetGuid() == version1.Id);
        var component = Assert.Single(option.GetProperty("components").EnumerateArray());
        Assert.Equal("Rice", component.GetProperty("name").GetString());
        Assert.Equal(1m, component.GetProperty("quantity").GetDecimal());
        Assert.Equal("kg", component.GetProperty("unit").GetString());

        // 2. Select two supported service + platform-unit version pairs
        using var firstPut = await PutWithAntiforgeryAsync(repClient, $"/api/v1/recipients/{recipientId}/services", new
        {
            Services = new[]
            {
                new { ServiceVersionId = service1.Id, StandardVersionId = version1.Id },
                new { ServiceVersionId = service2.Id, StandardVersionId = version2.Id }
            }
        });
        var firstPutBody = await WorkflowDbFixture.JsonAsync(firstPut, HttpStatusCode.OK);
        var selectedAfterFirst = firstPutBody.GetProperty("selectedServices").EnumerateArray().ToList();
        Assert.Equal(2, selectedAfterFirst.Count);
        var firstPairId = selectedAfterFirst
            .Single(x => x.GetProperty("serviceVersionId").GetGuid() == service1.Id)
            .GetProperty("charityServiceId").GetGuid();

        // 3. Deselect service2 and keep service1 -> service2 is disabled, service1 keeps its CharityServiceId
        using var secondPut = await PutWithAntiforgeryAsync(repClient, $"/api/v1/recipients/{recipientId}/services", new
        {
            Services = new[]
            {
                new { ServiceVersionId = service1.Id, StandardVersionId = version1.Id }
            }
        });
        var secondPutBody = await WorkflowDbFixture.JsonAsync(secondPut, HttpStatusCode.OK);
        var selectedAfterSecond = secondPutBody.GetProperty("selectedServices").EnumerateArray().ToList();
        Assert.Single(selectedAfterSecond);
        Assert.Equal(firstPairId, selectedAfterSecond[0].GetProperty("charityServiceId").GetGuid());

        var rowsInDb = await db.CharityServices.AsNoTracking()
            .Where(cs => cs.CharityId == recipientId)
            .ToListAsync();
        Assert.Equal(2, rowsInDb.Count);
        Assert.Single(rowsInDb, r => r.ServiceVersionId == service1.Id && r.Enabled);
        Assert.Single(rowsInDb, r => r.ServiceVersionId == service2.Id && !r.Enabled);
    }

    [Fact]
    public async Task CharityServiceSelection_RejectsMismatchedOrUnpublishedVersions()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var (_, service1, _, version1) = await PublishedCatalogAsync(db);
        var (_, service2, _, _) = await PublishedCatalogAsync(db);

        var draftCategory = new ServiceCategory { Code = $"DRAFT-{Guid.NewGuid():N}"[..20] };
        var draftService = new ServiceVersion { CategoryId = draftCategory.Id, VersionNumber = 1, IsPublished = false, Name = "Draft Service" };
        var draftUnit = new UnitStandard { ServiceVersionId = draftService.Id, Name = "Draft Unit" };
        var draftVersion = new StandardVersion { UnitStandardId = draftUnit.Id, VersionNumber = 1, IsPublished = false, EffectiveAt = DateTimeOffset.UtcNow };
        db.ServiceCategories.Add(draftCategory);
        db.ServiceVersions.Add(draftService);
        db.UnitStandards.Add(draftUnit);
        db.StandardVersions.Add(draftVersion);
        await db.SaveChangesAsync();

        var repClient = await CreateLoggedInCharityRepresentativeAsync("Rep Validation");
        var charity = await WorkflowDbFixture.RecipientAsync(repClient);
        var recipientId = charity.GetProperty("id").GetGuid();

        // Mismatched serviceVersionId and standardVersionId
        using var mismatchResponse = await PutWithAntiforgeryAsync(repClient, $"/api/v1/recipients/{recipientId}/services", new
        {
            Services = new[]
            {
                new { ServiceVersionId = service2.Id, StandardVersionId = version1.Id }
            }
        });
        Assert.Equal(HttpStatusCode.BadRequest, mismatchResponse.StatusCode);

        // Unpublished service & standard version
        using var unpublishedResponse = await PutWithAntiforgeryAsync(repClient, $"/api/v1/recipients/{recipientId}/services", new
        {
            Services = new[]
            {
                new { ServiceVersionId = draftService.Id, StandardVersionId = draftVersion.Id }
            }
        });
        Assert.Equal(HttpStatusCode.BadRequest, unpublishedResponse.StatusCode);
    }

    [Fact]
    public async Task CharityServiceSelection_PreventsUnrelatedAndUnauthorizedAccess()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var (_, service1, _, version1) = await PublishedCatalogAsync(db);

        var ownerClient = await CreateLoggedInCharityRepresentativeAsync("Rep Owner");
        var charityA = await WorkflowDbFixture.RecipientAsync(ownerClient);
        var charityAId = charityA.GetProperty("id").GetGuid();

        var otherRepClient = await CreateLoggedInCharityRepresentativeAsync("Rep Other");
        await WorkflowDbFixture.RecipientAsync(otherRepClient);

        // Unrelated charity representative cannot view or update charityA's services
        using var otherGet = await otherRepClient.GetAsync($"/api/v1/recipients/{charityAId}/services");
        Assert.Equal(HttpStatusCode.NotFound, otherGet.StatusCode);

        using var otherPut = await PutWithAntiforgeryAsync(otherRepClient, $"/api/v1/recipients/{charityAId}/services", new
        {
            Services = new[] { new { ServiceVersionId = service1.Id, StandardVersionId = version1.Id } }
        });
        Assert.Equal(HttpStatusCode.NotFound, otherPut.StatusCode);

        // Beneficiary (non-RecipientRepresentative) is blocked from viewing and updating
        var beneficiaryClient = fixture.Client();
        var benEmail = $"ben-{Guid.NewGuid():N}@example.test";
        var regBen = await WorkflowDbFixture.RegisterBeneficiaryAsync(beneficiaryClient, "Fictional Beneficiary", benEmail, fixture.Password);
        Assert.Equal(HttpStatusCode.Created, regBen.StatusCode);
        var loginBen = await WorkflowDbFixture.LoginRegisteredAsync(beneficiaryClient, benEmail, fixture.Password);
        Assert.Equal(HttpStatusCode.NoContent, loginBen.StatusCode);

        using var benGet = await beneficiaryClient.GetAsync($"/api/v1/recipients/{charityAId}/services");
        Assert.Equal(HttpStatusCode.NotFound, benGet.StatusCode);

        using var benPut = await PutWithAntiforgeryAsync(beneficiaryClient, $"/api/v1/recipients/{charityAId}/services", new
        {
            Services = new[] { new { ServiceVersionId = service1.Id, StandardVersionId = version1.Id } }
        });
        Assert.Equal(HttpStatusCode.Forbidden, benPut.StatusCode);

        // Anonymous request gets 401 Unauthorized
        var anonClient = fixture.Client();
        using var anonGet = await anonClient.GetAsync($"/api/v1/recipients/{charityAId}/services");
        Assert.Equal(HttpStatusCode.Unauthorized, anonGet.StatusCode);
    }

    private async Task<HttpClient> CreateLoggedInCharityRepresentativeAsync(string displayName)
    {
        var client = fixture.Client();
        var email = $"rep-{Guid.NewGuid():N}@example.test";
        var registerResponse = await WorkflowDbFixture.RegisterCharityAsync(
            client,
            displayName,
            email,
            fixture.Password,
            $"{displayName} Charity",
            "fictional@example.test",
            "REG-P119");
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await WorkflowDbFixture.LoginRegisteredAsync(client, email, fixture.Password);
        Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);
        return client;
    }

    private static async Task<HttpResponseMessage> PutWithAntiforgeryAsync(HttpClient client, string path, object body)
    {
        using var tokenResponse = await client.GetAsync("/api/v1/auth/antiforgery");
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
        using var request = new HttpRequestMessage(HttpMethod.Put, path) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return await client.SendAsync(request);
    }
}
