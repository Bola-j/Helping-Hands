using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Reminders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private async Task<JsonElement> CreateReferralAsync(
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
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var user = await db.Users.SingleAsync(x => x.NormalizedEmail == fixture.Beneficiary.ToUpperInvariant());
        var legacy = new AssistanceRequest
        {
            BeneficiaryId = Id(profile), SubmittedByUserId = user.Id, Description = "Fictional request",
            Reference = await db.NextReferenceAsync("REQ"), AuthorityConfirmed = true, SharingAgreed = true
        };
        db.Requests.Add(legacy);
        db.DecisionHistory.Add(new DecisionHistory { RequestId = legacy.Id, ActorUserId = user.Id, Action = "Submitted" });
        await db.SaveChangesAsync();
        using var requestResponse = await beneficiary.GetAsync($"/api/v1/requests/{legacy.Id}");
        requestResponse.EnsureSuccessStatusCode();
        var request = await requestResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accepted = await EvidenceFlowFixture.PostJsonAsync(coordinator, $"/api/v1/requests/{Id(request)}/decisions", new
        {
            Decision = "Accepted",
            Category = "Goods",
            RequiredQuantity = 1,
            Unit = "box",
            Version = Version(request)
        }, HttpStatusCode.OK);
        var (serviceVersionId, standardVersionId) = await SeedReferralSupportAsync(charity);
        var mapped = await EvidenceFlowFixture.PostJsonAsync(coordinator, $"/api/v1/requests/{Id(request)}/legacy-standard", new
        {
            StandardVersionId = standardVersionId,
            Version = Version(accepted)
        }, HttpStatusCode.OK);
        return await EvidenceFlowFixture.PostJsonAsync(administrator, $"/api/v1/requests/{Id(request)}/referrals", new
        {
            RecipientId = charity,
            AssignedQuantity = 1,
            ScopeDescription = "Fictional goods",
            Version = Version(mapped),
            ServiceVersionId = serviceVersionId,
            StandardVersionId = standardVersionId
        }, HttpStatusCode.Created);
    }
}
