using System.Net;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Referrals;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task<SplitReferralAllocationResult> AllocateNeedAsync(AcceptedNeedAllocationScenario scenario)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var allocator = scope.ServiceProvider.GetRequiredService<SplitReferralAllocationService>();
        return await allocator.AllocateSplitAsync(new(scenario.RequestId, scenario.NeedId, scenario.ActorUserId,
            [new(scenario.RecipientId, scenario.ServiceVersionId, scenario.StandardVersionId, 1m,
                "Fictional accepted need allocation")]));
    }

    private async Task AssertNeedStateAsync(AcceptedNeedAllocationScenario scenario)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        Assert.Equal("Open", (await database.Requests.SingleAsync(item => item.Id == scenario.RequestId)).Status);
        Assert.Equal("Accepted", (await database.RequestNeeds.SingleAsync(item => item.Id == scenario.NeedId)).Status);
        Assert.Equal("Rejected", (await database.RequestNeeds.SingleAsync(item => item.Id == scenario.RejectedNeedId)).Status);
    }

    private async Task AssertOutcomeReviewAllowedAsync(AcceptedNeedAllocationScenario scenario, string referralReference)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var referralId = await database.Referrals.Where(item => item.Reference == referralReference)
            .Select(item => item.Id).SingleAsync();
        var outcome = new Outcome { ReferralId = referralId, ReportedByUserId = fixture.UserIds["RecipientRepresentative"],
            Reference = await database.NextReferenceAsync("OUT"), ReportedQuantity = 1m,
            Description = "Fictional submitted outcome", Status = "Submitted", SubmittedAt = DateTimeOffset.UtcNow };
        database.Outcomes.Add(outcome);
        await database.SaveChangesAsync();
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var response = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/outcomes/{outcome.Id}/reviews",
            new { Decision = "Unresolved", Version = outcome.Version });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
