using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task SingleNeedRequest_RequiresBothConsents()
    {
        var self = await RegisterSelfBeneficiaryAsync();
        using var client = self.Client;
        using var response = await WorkflowDbFixture.PostAsync(client, "/api/v1/requests/single-need", new
        { BeneficiaryId = self.BeneficiaryId, Description = "Fictional request", AuthorityConfirmed = false, SharingAgreed = true });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SingleNeedRequest_RequiresBeneficiaryRole()
    {
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var response = await WorkflowDbFixture.PostAsync(coordinator, "/api/v1/requests/single-need", new
        { BeneficiaryId = Guid.NewGuid(), Description = "Fictional request", AuthorityConfirmed = true, SharingAgreed = true });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SingleNeedRequest_CannotUseAnotherBeneficiaryProfile()
    {
        var owner = await RegisterSelfBeneficiaryAsync();
        var other = await RegisterSelfBeneficiaryAsync();
        using var ownerClient = owner.Client;
        using var otherClient = other.Client;
        using var response = await WorkflowDbFixture.PostAsync(ownerClient, "/api/v1/requests/single-need", new
        { BeneficiaryId = other.BeneficiaryId, Description = "Fictional request", AuthorityConfirmed = true, SharingAgreed = true });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

}
