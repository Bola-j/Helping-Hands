using System.Net;
using HelpingHands.Api.Features.Needs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task UnrelatedUserCannotReadOrAnswerNeedClarification()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, request) = await NeedRequestAsync(scope);
        var (_, _, _, version) = await PublishedCatalogAsync(db);
        await using (var tx = await db.Database.BeginTransactionAsync())
        {
            Assert.Equal(NeedCreationResult.Created, await scope.ServiceProvider.GetRequiredService<NeedVersionService>().AddNeedsAsync(request.Id, [new(version.Id, 1)]));
            await tx.CommitAsync();
        }
        var id = await db.RequestNeeds.Where(x => x.RequestId == request.Id).Select(x => x.Id).SingleAsync();
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var unrelated = await fixture.LoginAsync("Unrelated");
        var path = $"/api/v1/requests/{request.Id}/needs/{id}";
        using var decision = await WorkflowDbFixture.PostAsync(coordinator, path + "/decisions", new { Decision = "Clarification", Version = request.Version });
        var asked = await WorkflowDbFixture.JsonAsync(decision, HttpStatusCode.OK);
        using var denied = await WorkflowDbFixture.PostAsync(unrelated, path + "/clarifications", new { Explanation = "Unrelated", Version = asked.GetProperty("version").GetGuid() });
        Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode);
        using var history = await unrelated.GetAsync(path + "/history");
        Assert.Equal(HttpStatusCode.NotFound, history.StatusCode);
        Assert.Equal(1, await db.NeedReviewHistory.CountAsync(x => x.NeedId == id));
    }
}
