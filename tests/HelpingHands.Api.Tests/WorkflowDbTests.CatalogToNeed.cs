using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task CoordinatorCatalogToBeneficiaryRequestPreservesVersionAfterNeedReview()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (_, profile) = await LinkedProfileAsync(scope);
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        var category = await CatalogPostAsync(coordinator, "/api/v1/catalog/categories", new { Code = Guid.NewGuid().ToString("N") }, HttpStatusCode.Created);
        var service = await CatalogPostAsync(coordinator, $"/api/v1/catalog/categories/{category.GetProperty("id").GetGuid()}/versions", new { Name = "Fictional food" }, HttpStatusCode.Created);
        var serviceId = service.GetProperty("id").GetGuid();
        var standard = await CatalogPostAsync(coordinator, $"/api/v1/catalog/service-versions/{serviceId}/standards", new { Name = "Fictional parcel" }, HttpStatusCode.Created);
        var standardId = standard.GetProperty("id").GetGuid();
        var version = await CatalogPostAsync(coordinator, $"/api/v1/catalog/standards/{standardId}/versions", new { EffectiveAt = DateTimeOffset.UtcNow.AddDays(-2) }, HttpStatusCode.Created);
        var versionId = version.GetProperty("id").GetGuid();
        await CatalogPostAsync(coordinator, $"/api/v1/catalog/standard-versions/{versionId}/components", new { Name = "Rice", Quantity = 2, Unit = "kg" }, HttpStatusCode.Created);
        using (var published = await CatalogPublishAsync(coordinator, $"/api/v1/catalog/service-versions/{serviceId}")) Assert.Equal(HttpStatusCode.OK, published.StatusCode);
        using (var published = await CatalogPublishAsync(coordinator, $"/api/v1/catalog/standard-versions/{versionId}")) Assert.Equal(HttpStatusCode.OK, published.StatusCode);
        using var submission = await PostNeedsAsync(beneficiary, profile.Id, new[] { new { StandardVersionId = versionId, Quantity = 1 } });
        var request = await WorkflowDbFixture.JsonAsync(submission, HttpStatusCode.Created);
        var requestId = request.GetProperty("id").GetGuid();
        var needId = request.GetProperty("needs")[0].GetProperty("id").GetGuid();
        var next = await CatalogPostAsync(coordinator, $"/api/v1/catalog/standards/{standardId}/versions", new { EffectiveAt = DateTimeOffset.UtcNow.AddDays(-1) }, HttpStatusCode.Created);
        var nextId = next.GetProperty("id").GetGuid();
        await CatalogPostAsync(coordinator, $"/api/v1/catalog/standard-versions/{nextId}/components", new { Name = "Beans", Quantity = 1, Unit = "kg" }, HttpStatusCode.Created);
        using (var published = await CatalogPublishAsync(coordinator, $"/api/v1/catalog/standard-versions/{nextId}")) Assert.Equal(HttpStatusCode.OK, published.StatusCode);
        using var decision = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/requests/{requestId}/needs/{needId}/decisions", new { Decision = "Accepted", Category = "Goods", Version = request.GetProperty("version").GetGuid() });
        var reviewed = await WorkflowDbFixture.JsonAsync(decision, HttpStatusCode.OK);
        Assert.Equal("FullyAccepted", reviewed.GetProperty("status").GetString());
        var saved = await beneficiary.GetFromJsonAsync<JsonElement>($"/api/v1/requests/{requestId}");
        var recorded = saved.GetProperty("needs")[0];
        Assert.Equal(versionId, recorded.GetProperty("standardVersionId").GetGuid());
        Assert.Equal("Rice", recorded.GetProperty("components")[0].GetProperty("name").GetString());
    }
}
