using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

[Trait("Category", "PostgresIntegration")]
public sealed partial class WorkflowDbTests(WorkflowDbFixture fixture) : IClassFixture<WorkflowDbFixture>
{
    [Fact]
    public async Task CharityApproval_RequiresAdministratorAndConcealsUnrelatedRecords()
    {
        using var applicant = await fixture.LoginAsync("Requester");
        using var unrelated = await fixture.LoginAsync("Unrelated");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var admin = await fixture.LoginAsync("Administrator");
        var charity = await WorkflowDbFixture.RecipientAsync(applicant);
        var id = charity.GetProperty("id").GetGuid();
        var version = charity.GetProperty("version").GetGuid();
        using var hidden = await unrelated.GetAsync($"/api/v1/recipients/{id}");
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
        using var forbidden = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/recipients/{id}/decisions", new { Decision = "Approved", Version = version });
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        var approved = await WorkflowDbFixture.ApproveAsync(admin, charity);
        Assert.Equal("Approved", approved.GetProperty("approvalStatus").GetString());
        Assert.True(approved.GetProperty("active").GetBoolean());
        using var exact = await admin.GetAsync("/api/v1/recipients?reference=" + charity.GetProperty("reference").GetString());
        var matches = await exact.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(matches.EnumerateArray(), item => item.GetProperty("id").GetGuid() == id);
    }
}
