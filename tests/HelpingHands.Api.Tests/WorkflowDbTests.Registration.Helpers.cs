using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task<Guid> RegisterAndLoginBeneficiaryAsync(HttpClient client)
    {
        var email = $"requester-{Guid.NewGuid():N}@example.test";
        var password = $"{Guid.NewGuid():N}Aa1!";
        using var created = await WorkflowDbFixture.RegisterBeneficiaryAsync(client, "Fictional requester", email, password, "Fictional contact", true);
        var account = await WorkflowDbFixture.JsonAsync(created, HttpStatusCode.Created);
        var id = account.GetProperty("accountId").GetGuid();
        Assert.NotEqual(Guid.Empty, account.GetProperty("beneficiaryId").GetGuid());
        Assert.StartsWith("BEN-", account.GetProperty("reference").GetString());
        Assert.NotEqual(Guid.Empty, id);
        Assert.DoesNotContain(password, account.GetRawText());
        using var login = await WorkflowDbFixture.LoginRegisteredAsync(client, email, password);
        Assert.True(login.StatusCode == HttpStatusCode.NoContent, await login.Content.ReadAsStringAsync());
        using var profileResponse = await client.GetAsync("/api/v1/auth/me");
        var profile = await WorkflowDbFixture.JsonAsync(profileResponse, HttpStatusCode.OK);
        Assert.Contains(profile.GetProperty("roles").EnumerateArray(), role => role.GetString() == "Beneficiary");
        return id;
    }
}
