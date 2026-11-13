using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task BeneficiaryCannotPromoteSelfToRecipientRepresentative()
    {
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        using var response = await WorkflowDbFixture.PostAsync(beneficiary, "/api/v1/recipients", new
        { DisplayName = "Fictional charity", ContactDetails = "Fictional contact", RegistrationInformation = "Fictional registration" });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
