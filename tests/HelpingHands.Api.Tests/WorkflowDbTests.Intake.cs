using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task Intake_RequiresConfirmationsAndConcealsRequestFromUnrelatedUser()
    {
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var unrelated = await fixture.LoginAsync("Unrelated");
        var beneficiary = await WorkflowDbFixture.BeneficiaryAsync(coordinator);
        var id = beneficiary.GetProperty("id").GetGuid();
        using var invalid = await WorkflowDbFixture.PostAsync(coordinator, "/api/v1/requests", new
        { BeneficiaryId = id, Description = "Fictional need", AuthorityConfirmed = false, SharingAgreed = true });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var request = await WorkflowDbFixture.RequestAsync(coordinator, id);
        using var hidden = await unrelated.GetAsync($"/api/v1/requests/{request.GetProperty("id").GetGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
        using var history = await coordinator.GetAsync($"/api/v1/requests/{request.GetProperty("id").GetGuid()}/history");
        var entries = await history.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(entries.EnumerateArray(), item => item.GetProperty("action").GetString() == "Submitted");
    }
}
