using System.Net;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private static Task<JsonElement> CreatePublicFlowReferralAsync(HttpClient administrator, JsonElement request,
        JsonElement mapped, Guid charity, Guid serviceVersionId, Guid standardVersionId) =>
        EvidenceFlowFixture.PostJsonAsync(administrator, $"/api/v1/requests/{Id(request)}/referrals", new
        {
            RecipientId = charity, AssignedQuantity = 1, ScopeDescription = "Fictional goods",
            Version = Version(mapped), ServiceVersionId = serviceVersionId, StandardVersionId = standardVersionId
        }, HttpStatusCode.Created);
}
