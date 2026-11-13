using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowFixture
{
    public async Task RegisterCharityAndLoginAsync(HttpClient client, string email)
    {
        await PostJsonAsync(client, "/api/v1/auth/register/charity", new
        {
            RepresentativeDisplayName = "Fictional representative",
            Email = email,
            Password,
            OrganizationName = "Fictional charity",
            ContactDetails = "Fictional contact",
            RegistrationInformation = "Fictional registration"
        }, HttpStatusCode.Created);
        await LoginAsync(client, email);
    }
}
