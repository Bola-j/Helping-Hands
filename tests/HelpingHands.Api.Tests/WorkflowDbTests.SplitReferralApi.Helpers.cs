using System.Net;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Referrals;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

internal static class SplitReferralApiHelpers
{
    public static string Path(Guid requestId, Guid needId) => $"/api/v1/requests/{requestId}/needs/{needId}/referrals";
    public static SplitAllocationLegInput Leg(AcceptedNeedAllocationScenario scenario, decimal quantity) =>
        new(scenario.RecipientId, scenario.ServiceVersionId, scenario.StandardVersionId, quantity, "Fictional need allocation");

    public static async Task<Guid> RequestVersionAsync(WorkflowDbFixture fixture, Guid requestId)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>().Requests
            .Where(item => item.Id == requestId).Select(item => item.Version).SingleAsync();
    }
}

internal static class SplitReferralApiAssertions
{
    public static async Task RejectAsync(HttpClient coordinator, string path, Guid version, SplitAllocationLegInput[] legs)
    {
        using var response = await WorkflowDbFixture.PostAsync(coordinator, path, new { Version = version, Allocations = legs });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    public static async Task ReferralsAsync(WorkflowDbFixture fixture, AcceptedNeedAllocationScenario scenario, int count, decimal assigned, Guid[]? ids = null)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var referrals = await db.Referrals.Where(item => item.RequestId == scenario.RequestId).ToListAsync();
        Assert.Equal(count, referrals.Count);
        Assert.Equal(assigned, referrals.Sum(item => item.AssignedQuantity));
        Assert.All(referrals, item => Assert.Equal(scenario.NeedId, item.NeedId));
        if (ids is not null) Assert.Equal(ids.Order(), referrals.Select(item => item.Id).Order());
    }
}
