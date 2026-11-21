using System.Net;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private async Task<Guid> OpenSessionsAsync(HttpClient admin, HttpClient coordinator, HttpClient representative, HttpClient beneficiary, HttpClient outsider)
    {
        await fixture.LoginAsync(admin, fixture.Administrator);
        await fixture.LoginAsync(coordinator, fixture.Coordinator);
        await fixture.RegisterCharityAndLoginAsync(representative, fixture.Representative);
        var signup = await fixture.RegisterAndLoginAsync(beneficiary, fixture.Beneficiary);
        await fixture.RegisterAndLoginAsync(outsider, fixture.Outsider);
        return signup.GetProperty("beneficiaryId").GetGuid();
    }

    private async Task<Guid> CreateApprovedCharityAsync(HttpClient admin, HttpClient representative)
    {
        using var listResponse = await representative.GetAsync("/api/v1/recipients");
        var list = await EvidenceFlowFixture.JsonAsync(listResponse, HttpStatusCode.OK);
        var id = list.EnumerateArray().Single().GetProperty("id").GetGuid();
        using var detailResponse = await representative.GetAsync($"/api/v1/recipients/{id}");
        var created = await EvidenceFlowFixture.JsonAsync(detailResponse, HttpStatusCode.OK);
        await EvidenceFlowFixture.UploadApplicationAsync(representative, id);
        var approved = await EvidenceFlowFixture.PostJsonAsync(admin, $"/api/v1/recipients/{Id(created)}/decisions",
            new { Decision = "Approved", Reason = (string?)null, Version = Version(created) }, HttpStatusCode.OK);
        Assert.Equal("Approved", approved.GetProperty("approvalStatus").GetString());
        return Id(created);
    }
}
