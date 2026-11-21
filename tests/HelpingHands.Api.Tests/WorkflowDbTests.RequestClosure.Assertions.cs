using System.Net;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task AssertRequestNotClosedAsync(Guid requestId, Guid version)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var request = await database.Requests.SingleAsync(item => item.Id == requestId);
        Assert.Equal("Open", request.Status);
        Assert.Equal(version, request.Version);
        Assert.Empty(await database.DecisionHistory.Where(item => item.RequestId == requestId && item.Action == "RequestClosed").ToListAsync());
    }

    private async Task SeedAcceptedOutcomeAsync(Guid referralId)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var referral = await database.Referrals.SingleAsync(item => item.Id == referralId);
        referral.Status = "Completed";
        var outcome = new Outcome { ReferralId = referralId, ReportedByUserId = fixture.UserIds["RecipientRepresentative"],
            Reference = await database.NextReferenceAsync("OUT"), ReportedQuantity = 1m,
            Description = "Fictional accepted delivery", Status = "Accepted" };
        database.Outcomes.Add(outcome);
        database.OutcomeReviews.Add(new OutcomeReview { OutcomeId = outcome.Id,
            ActorUserId = fixture.UserIds["Coordinator"], Decision = "Accepted" });
        await database.SaveChangesAsync();
    }

    private async Task<Guid> AssertConcurrentClosureAsync(string path, Guid version, HttpClient coordinator, HttpClient admin)
    {
        var attempts = await Task.WhenAll(
            WorkflowDbFixture.PostAsync(coordinator, path, new { Version = version }),
            WorkflowDbFixture.PostAsync(admin, path, new { Version = version }));
        try
        {
            Assert.Equal(1, attempts.Count(item => item.StatusCode == HttpStatusCode.NoContent));
            Assert.Equal(1, attempts.Count(item => item.StatusCode == HttpStatusCode.Conflict));
            return attempts[0].StatusCode == HttpStatusCode.NoContent
                ? fixture.UserIds["Coordinator"] : fixture.UserIds["Administrator"];
        }
        finally { foreach (var attempt in attempts) attempt.Dispose(); }
    }
}
