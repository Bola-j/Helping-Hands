using System.Net;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowFixture
{
    public async Task<JsonElement> RegisterAndLoginAsync(HttpClient client, string email)
    {
        var signup = await PostJsonAsync(client, "/api/v1/auth/register/beneficiary", new { DisplayName = "Fictional user", email, Password, ContactDetails = (string?)null, IsFamily = false }, HttpStatusCode.Created);
        await LoginAsync(client, email);
        return signup;
    }

    public async Task RefreshLoginAsync(HttpClient client, string email)
    {
        await PostJsonAsync(client, "/api/v1/auth/logout", new { }, HttpStatusCode.NoContent);
        await LoginAsync(client, email);
    }
}
