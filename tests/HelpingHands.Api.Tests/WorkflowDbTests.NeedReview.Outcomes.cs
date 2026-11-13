using System.Net;
using HelpingHands.Api.Features.Needs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Theory]
    [InlineData("Accepted", "FullyAccepted")]
    [InlineData("Rejected", "FullyRejected")]
    public async Task SingleNeedReviewProducesFinalRequestOutcome(string decision, string expected)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, request) = await NeedRequestAsync(scope);
        var (_, _, _, version) = await PublishedCatalogAsync(db);
        await using (var tx = await db.Database.BeginTransactionAsync())
        {
            Assert.Equal(NeedCreationResult.Created, await scope.ServiceProvider.GetRequiredService<NeedVersionService>().AddNeedsAsync(request.Id, [new(version.Id, 1)]));
            await tx.CommitAsync();
        }
        var needId = await db.RequestNeeds.Where(x => x.RequestId == request.Id).Select(x => x.Id).SingleAsync();
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var result = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/requests/{request.Id}/needs/{needId}/decisions", new { Decision = decision, Category = "Goods", Reason = "Fictional", Version = request.Version });
        var payload = await WorkflowDbFixture.JsonAsync(result, HttpStatusCode.OK);
        Assert.Equal(expected, payload.GetProperty("status").GetString());
    }

    [Fact]
    public async Task ConcurrentNeedReviewsWithSameRequestVersionHaveOneWinner()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, request) = await NeedRequestAsync(scope);
        var (_, _, _, first) = await PublishedCatalogAsync(db);
        var (_, _, _, second) = await PublishedCatalogAsync(db);
        await using (var tx = await db.Database.BeginTransactionAsync())
        {
            Assert.Equal(NeedCreationResult.Created, await scope.ServiceProvider.GetRequiredService<NeedVersionService>().AddNeedsAsync(request.Id, [new(first.Id, 1), new(second.Id, 1)]));
            await tx.CommitAsync();
        }
        var ids = await db.RequestNeeds.Where(x => x.RequestId == request.Id).Select(x => x.Id).ToListAsync();
        using var left = await fixture.LoginAsync("Coordinator");
        using var right = await fixture.LoginAsync("Coordinator");
        var calls = ids.Select((id, i) => WorkflowDbFixture.PostAsync(i == 0 ? left : right, $"/api/v1/requests/{request.Id}/needs/{id}/decisions", new { Decision = "Accepted", Category = "Goods", Version = request.Version }));
        var results = await Task.WhenAll(calls);
        Assert.Single(results, x => x.StatusCode == HttpStatusCode.OK);
        Assert.Single(results, x => x.StatusCode == HttpStatusCode.Conflict);
        foreach (var result in results) result.Dispose();
        Assert.Equal(1, await db.NeedReviewHistory.CountAsync(x => ids.Contains(x.NeedId)));
    }
}
