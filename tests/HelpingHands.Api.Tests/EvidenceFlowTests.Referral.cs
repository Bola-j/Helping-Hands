using System.Net;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private static async Task<JsonElement> CreateReferralAsync(
        HttpClient coordinator,
        HttpClient administrator,
        HttpClient beneficiary,
        Guid charity)
    {
        var profile = await EvidenceFlowFixture.PostJsonAsync(beneficiary, "/api/v1/beneficiaries",
            new
            {
                DisplayName = "Fictional beneficiary",
                ContactDetails = "Fictional contact",
                IsFamily = false
            }, HttpStatusCode.Created);
        var request = await EvidenceFlowFixture.PostJsonAsync(beneficiary, "/api/v1/requests", new
        {
            BeneficiaryId = Id(profile),
            Description = "Fictional request",
            AuthorityConfirmed = true,
            SharingAgreed = true
        }, HttpStatusCode.Created);
        var accepted = await EvidenceFlowFixture.PostJsonAsync(coordinator, $"/api/v1/requests/{Id(request)}/decisions", new
        {
            Decision = "Accepted",
            Category = "Goods",
            RequiredQuantity = 1,
            Unit = "box",
            Version = Version(request)
        }, HttpStatusCode.OK);
        return await EvidenceFlowFixture.PostJsonAsync(administrator, $"/api/v1/requests/{Id(request)}/referrals", new
        {
            RecipientId = charity,
            AssignedQuantity = 1,
            ScopeDescription = "Fictional goods",
            Version = Version(accepted)
        }, HttpStatusCode.Created);
    }
}
