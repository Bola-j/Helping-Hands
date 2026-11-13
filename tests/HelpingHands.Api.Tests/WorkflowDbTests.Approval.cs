using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

[Trait("Category", "PostgresIntegration")]
public sealed partial class WorkflowDbTests(WorkflowDbFixture fixture) : IClassFixture<WorkflowDbFixture>
{
    [Fact]
    public async Task CharityReviewAllowsCoordinatorAndAdministratorAndConcealsUnrelatedRecords()
    {
        using var applicant = await fixture.LoginAsync("RecipientRepresentative");
        using var unrelated = await fixture.LoginAsync("Unrelated");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var admin = await fixture.LoginAsync("Administrator");
        var charity = await WorkflowDbFixture.RecipientAsync(applicant);
        var id = charity.GetProperty("id").GetGuid();
        using var hidden = await unrelated.GetAsync($"/api/v1/recipients/{id}");
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
        var approved = await WorkflowDbFixture.ApproveAsync(coordinator, charity);
        Assert.Equal("Approved", approved.GetProperty("approvalStatus").GetString());
        Assert.True(approved.GetProperty("active").GetBoolean());
        var secondCharity = await WorkflowDbFixture.RecipientAsync(applicant);
        var adminApproved = await WorkflowDbFixture.ApproveAsync(admin, secondCharity);
        Assert.Equal("Approved", adminApproved.GetProperty("approvalStatus").GetString());
        using var exact = await admin.GetAsync("/api/v1/recipients?reference=" + charity.GetProperty("reference").GetString());
        var matches = await exact.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(matches.EnumerateArray(), item => item.GetProperty("id").GetGuid() == id);
    }
}
