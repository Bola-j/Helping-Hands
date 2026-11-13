using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task<Guid> RegisterAndLoginRequesterAsync(HttpClient client)
    {
        var email = $"requester-{Guid.NewGuid():N}@example.test";
        var password = $"{Guid.NewGuid():N}Aa1!";
        using var created = await WorkflowDbFixture.RegisterAsync(client, "Fictional requester", email, password);
        var account = await WorkflowDbFixture.JsonAsync(created, HttpStatusCode.Created);
        var id = account.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, id);
        Assert.DoesNotContain(password, account.GetRawText());
        using var login = await WorkflowDbFixture.LoginRegisteredAsync(client, email, password);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        using var profileResponse = await client.GetAsync("/api/v1/auth/me");
        var profile = await WorkflowDbFixture.JsonAsync(profileResponse, HttpStatusCode.OK);
        Assert.Contains(profile.GetProperty("roles").EnumerateArray(), role => role.GetString() == "Requester");
        return id;
    }
}
