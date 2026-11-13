using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private async Task<(Guid OutcomeId, Guid Version)> SubmitEvidenceAsync(HttpClient representative, HttpClient outsider, JsonElement referral)
    {
        var created = await EvidenceFlowFixture.PostJsonAsync(representative, $"/api/v1/referrals/{Id(referral)}/outcomes",
            new { ReportedQuantity = 1, Description = "Fictional delivery and receipt" }, HttpStatusCode.Created);
        var missingEvidence = await EvidenceFlowFixture.PostJsonAsync(representative, $"/api/v1/outcomes/{Id(created)}/submit",
            new { Version = Version(created) }, HttpStatusCode.BadRequest);
        Assert.Equal("An evidence attachment is required.", missingEvidence.GetProperty("detail").GetString());
        var attachment = await EvidenceFlowFixture.UploadAsync(representative, Id(created), HttpStatusCode.Created);
        await AssertAttachmentAccessAsync(representative, outsider, Id(attachment));
        var submitted = await EvidenceFlowFixture.PostJsonAsync(representative, $"/api/v1/outcomes/{Id(created)}/submit",
            new { Version = Version(created) }, HttpStatusCode.OK);
        Assert.Equal("Submitted", submitted.GetProperty("status").GetString());
        return (Id(created), Version(submitted));
    }

    private static async Task AcceptAndCheckHistoryAsync(HttpClient coordinator, Guid outcome, Guid version, JsonElement referral)
    {
        var accepted = await EvidenceFlowFixture.PostJsonAsync(coordinator, $"/api/v1/outcomes/{outcome}/reviews",
            new { Decision = "Accepted", Reason = (string?)null, Version = version }, HttpStatusCode.OK);
        Assert.Equal("Accepted", accepted.GetProperty("status").GetString());
        using var response = await coordinator.GetAsync($"/api/v1/referrals/{Id(referral)}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Completed", result.GetProperty("status").GetString());
        await AssertRequestAndHistoryAsync(coordinator, referral);
    }
}
