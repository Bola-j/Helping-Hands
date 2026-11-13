using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task LegacyRequesterCannotCreateBeneficiaryIntake()
    {
        using var owner = await fixture.LoginAsync("Beneficiary");
        using var requester = await fixture.LoginAsync("Requester");
        var profile = await WorkflowDbFixture.BeneficiaryAsync(owner);
        var existing = await WorkflowDbFixture.RequestAsync(owner, profile.GetProperty("id").GetGuid());
        using var forbiddenProfile = await WorkflowDbFixture.PostAsync(requester, "/api/v1/beneficiaries", new
        { DisplayName = "Fictional profile", ContactDetails = "Fictional contact", IsFamily = false });
        using var request = await WorkflowDbFixture.PostAsync(requester, "/api/v1/requests", new
        { BeneficiaryId = Guid.NewGuid(), Description = "Fictional need", AuthorityConfirmed = true, SharingAgreed = true });
        using var hidden = await requester.GetAsync($"/api/v1/requests/{existing.GetProperty("id").GetGuid()}");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenProfile.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, request.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
    }
}
