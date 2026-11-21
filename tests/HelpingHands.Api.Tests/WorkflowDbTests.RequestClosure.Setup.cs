using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task<MultiNeedClosureScenario> CreateMultiNeedClosureScenarioAsync()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (database, request) = await NeedRequestAsync(scope);
        using var applicant = await fixture.LoginAsync("RecipientRepresentative");
        using var admin = await fixture.LoginAsync("Administrator");
        var charity = await WorkflowDbFixture.RecipientAsync(applicant);
        await WorkflowDbFixture.ApproveAsync(admin, charity);
        request.Status = "Open";
        request.Version = Guid.NewGuid();
        var needs = await CreateClosureNeedsAsync(database, request);
        var referrals = await CreateClosureReferralsAsync(database, request.Id, needs, charity.GetProperty("id").GetGuid());
        return new(request.Id, request.Version, needs.Select(item => item.Id).ToArray(), referrals.Select(item => item.Id).ToArray());
    }

    private async Task<List<RequestNeed>> CreateClosureNeedsAsync(HelpingHandsDbContext database, AssistanceRequest request)
    {
        var first = await PublishedCatalogAsync(database);
        var second = await PublishedCatalogAsync(database);
        var needs = new List<RequestNeed>
        {
            new() { RequestId = request.Id, StandardVersionId = first.Version.Id, Quantity = 1m, Status = "Accepted" },
            new() { RequestId = request.Id, StandardVersionId = second.Version.Id, Quantity = 1m, Status = "Accepted" }
        };
        database.AddRange(needs);
        await database.SaveChangesAsync();
        return needs;
    }

    private async Task<Referral[]> CreateClosureReferralsAsync(HelpingHandsDbContext database, Guid requestId,
        IReadOnlyCollection<RequestNeed> needs, Guid recipientId)
    {
        var referrals = new List<Referral>();
        foreach (var need in needs)
            referrals.Add(new Referral { RequestId = requestId, NeedId = need.Id, RecipientId = recipientId,
                CreatedByUserId = fixture.UserIds["Coordinator"], AssignedQuantity = 1m,
                Reference = await database.NextReferenceAsync("REF"), ScopeDescription = "Fictional allocation" });
        database.AddRange(referrals);
        await database.SaveChangesAsync();
        return referrals.ToArray();
    }

}

internal sealed record MultiNeedClosureScenario(Guid RequestId, Guid Version, Guid[] NeedIds, Guid[] ReferralIds);
