using System.Net;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private async Task OpenSessionsAsync(HttpClient admin, HttpClient coordinator, HttpClient representative, HttpClient outsider)
    {
        await fixture.LoginAsync(admin, fixture.Administrator);
        await fixture.LoginAsync(coordinator, fixture.Coordinator);
        await fixture.RegisterAndLoginAsync(representative, fixture.Representative);
        await fixture.RegisterAndLoginAsync(outsider, fixture.Outsider);
    }

    private async Task<Guid> CreateApprovedCharityAsync(HttpClient admin, HttpClient representative)
    {
        var created = await EvidenceFlowFixture.PostJsonAsync(representative, "/api/v1/recipients", new
        {
            DisplayName = "Fictional charity", ContactDetails = "Fictional contact", RegistrationInformation = "Fictional registration"
        }, HttpStatusCode.Created);
        await fixture.RefreshLoginAsync(representative, fixture.Representative);
        var approved = await EvidenceFlowFixture.PostJsonAsync(admin, $"/api/v1/recipients/{Id(created)}/decisions",
            new { Decision = "Approved", Reason = (string?)null, Version = Version(created) }, HttpStatusCode.OK);
        Assert.Equal("Approved", approved.GetProperty("approvalStatus").GetString());
        return Id(created);
    }
}
