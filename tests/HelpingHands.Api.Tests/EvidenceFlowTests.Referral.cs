using System.Net;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private async Task<JsonElement> CreateReferralAsync(
        HttpClient coordinator,
        HttpClient administrator,
        HttpClient beneficiary,
        Guid beneficiaryId,
        Guid charity)
    {
        var request = await SubmitSingleNeedRequestAsync(beneficiary, beneficiaryId);
        var accepted = await AcceptSingleNeedRequestAsync(coordinator, request);
        var (serviceVersionId, standardVersionId) = await SeedReferralSupportAsync(charity);
        var mapped = await MapSingleNeedStandardAsync(coordinator, request, accepted, standardVersionId);
        return await CreatePublicFlowReferralAsync(administrator, request, mapped, charity, serviceVersionId, standardVersionId);
    }
}
