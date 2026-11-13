using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task BeneficiaryCannotReadAnotherBeneficiaryRecords()
    {
        using var owner = await fixture.LoginAsync("Beneficiary");
        using var other = fixture.Client();
        var email = $"other-{Guid.NewGuid():N}@example.test";
        using var registration = await WorkflowDbFixture.RegisterBeneficiaryAsync(other, "Fictional other", email, fixture.Password);
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
        using var login = await WorkflowDbFixture.LoginRegisteredAsync(other, email, fixture.Password);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        var request = await SelfRequestAsync(owner);
        using var response = await other.GetAsync($"/api/v1/requests/{request.GetProperty("id").GetGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
