using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task<(HttpClient Client, Guid BeneficiaryId)> RegisterSelfBeneficiaryAsync()
    {
        var client = fixture.Client();
        var email = $"self-{Guid.NewGuid():N}@example.test";
        using var registration = await WorkflowDbFixture.RegisterBeneficiaryAsync(client, "Fictional beneficiary", email, fixture.Password);
        var profile = await WorkflowDbFixture.JsonAsync(registration, HttpStatusCode.Created);
        using var login = await WorkflowDbFixture.LoginRegisteredAsync(client, email, fixture.Password);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        return (client, profile.GetProperty("beneficiaryId").GetGuid());
    }
}
