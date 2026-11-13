using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task CharityDecision_RecordsHistoryAndRejectsInvalidTransitions()
    {
        using var applicant = await fixture.LoginAsync("RecipientRepresentative");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var outsider = await fixture.LoginAsync("Unrelated");
        var charity = await WorkflowDbFixture.RecipientAsync(applicant);
        var id = charity.GetProperty("id").GetGuid();
        var version = charity.GetProperty("version").GetGuid();
        await AssertDecisionFailuresAsync(applicant, coordinator, id, version);
        using var rejected = await DecideAsync(coordinator, id, version, "Rejected", "Fictional reason");
        Assert.Equal(HttpStatusCode.OK, rejected.StatusCode);
        using var repeated = await DecideAsync(coordinator, id, version, "Approved", null);
        Assert.Equal(HttpStatusCode.Conflict, repeated.StatusCode);
        await AssertDecisionDetailAsync(applicant, coordinator, outsider, id);
    }

    [Fact]
    public async Task CharityDecision_ConcurrentReviewsRecordOnlyOneDecision()
    {
        using var applicant = await fixture.LoginAsync("RecipientRepresentative");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var admin = await fixture.LoginAsync("Administrator");
        var charity = await WorkflowDbFixture.RecipientAsync(applicant);
        var id = charity.GetProperty("id").GetGuid();
        var version = charity.GetProperty("version").GetGuid();
        var responses = await Task.WhenAll(DecideAsync(coordinator, id, version, "Approved", null),
            DecideAsync(admin, id, version, "Rejected", "Fictional reason"));
        try { Assert.Equal(new[] { HttpStatusCode.OK, HttpStatusCode.Conflict }, responses.Select(x => x.StatusCode).Order().ToArray()); }
        finally { foreach (var response in responses) response.Dispose(); }
        using var detailResponse = await coordinator.GetAsync($"/api/v1/recipients/{id}");
        var detail = await WorkflowDbFixture.JsonAsync(detailResponse, HttpStatusCode.OK);
        Assert.Single(detail.GetProperty("decisions").EnumerateArray());
    }

    [Fact]
    public async Task CharityApproval_RequiresRegistrationInformationOrDocument()
    {
        using var applicant = await fixture.LoginAsync("RecipientRepresentative");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var response = await WorkflowDbFixture.PostAsync(applicant, "/api/v1/recipients", new
        { DisplayName = "Fictional", ContactDetails = "Fictional", RegistrationInformation = (string?)null });
        var charity = await WorkflowDbFixture.JsonAsync(response, HttpStatusCode.Created);
        using var decision = await DecideAsync(coordinator, charity.GetProperty("id").GetGuid(), charity.GetProperty("version").GetGuid(), "Approved", null);
        Assert.Equal(HttpStatusCode.BadRequest, decision.StatusCode);
    }
}
