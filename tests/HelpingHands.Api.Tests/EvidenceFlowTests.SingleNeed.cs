using System.Net;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private static async Task<JsonElement> SubmitSingleNeedRequestAsync(HttpClient beneficiary, Guid beneficiaryId)
    {
        var request = await EvidenceFlowFixture.PostJsonAsync(beneficiary, "/api/v1/requests/single-need", new
        {
            BeneficiaryId = beneficiaryId, Description = "Fictional request",
            AuthorityConfirmed = true, SharingAgreed = true
        }, HttpStatusCode.Created);
        Assert.False(request.GetProperty("isMultiNeed").GetBoolean());
        Assert.Equal("Submitted", request.GetProperty("status").GetString());
        return request;
    }

    private static Task<JsonElement> AcceptSingleNeedRequestAsync(HttpClient coordinator, JsonElement request) =>
        EvidenceFlowFixture.PostJsonAsync(coordinator, $"/api/v1/requests/{Id(request)}/decisions", new
        {
            Decision = "Accepted", Category = "Goods", RequiredQuantity = 1,
            Unit = "box", Version = Version(request)
        }, HttpStatusCode.OK);

    private static Task<JsonElement> MapSingleNeedStandardAsync(HttpClient coordinator, JsonElement request, JsonElement accepted, Guid standardVersionId) =>
        EvidenceFlowFixture.PostJsonAsync(coordinator, $"/api/v1/requests/{Id(request)}/legacy-standard", new
        { StandardVersionId = standardVersionId, Version = Version(accepted) }, HttpStatusCode.OK);
}
