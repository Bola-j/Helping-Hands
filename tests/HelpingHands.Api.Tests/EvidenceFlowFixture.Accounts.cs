using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowFixture
{
    public async Task RegisterAndLoginAsync(HttpClient client, string email)
    {
        await PostJsonAsync(client, "/api/v1/auth/register/beneficiary", new { DisplayName = "Fictional user", email, Password, ContactDetails = (string?)null, IsFamily = false }, HttpStatusCode.Created);
        await LoginAsync(client, email);
    }

    public async Task RefreshLoginAsync(HttpClient client, string email)
    {
        await PostJsonAsync(client, "/api/v1/auth/logout", new { }, HttpStatusCode.NoContent);
        await LoginAsync(client, email);
    }
}
