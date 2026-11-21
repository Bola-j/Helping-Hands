using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Needs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private sealed record ProgressSeed(Guid RequestId, Guid FirstNeedId, Guid SecondNeedId, Guid FirstReferralId, Guid SecondReferralId);

    private async Task<ProgressSeed> SeedRequestProgressAsync()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, request) = await NeedRequestAsync(scope);
        var needs = await CreateProgressNeedsAsync(db, request);
        return await CreateProgressAssignmentsAsync(db, request, needs);
    }

    private async Task<Guid> SeedUnassignedProgressRequestAsync()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (_, request) = await NeedRequestAsync(scope);
        return request.Id;
    }

    private static async Task<List<RequestNeed>> CreateProgressNeedsAsync(HelpingHandsDbContext db, AssistanceRequest request)
    {
        var (_, _, _, first) = await PublishedCatalogAsync(db);
        var (_, _, _, second) = await PublishedCatalogAsync(db);
        var service = new NeedVersionService(db);
        await using var transaction = await db.Database.BeginTransactionAsync();
        Assert.Equal(NeedCreationResult.Created, await service.AddNeedsAsync(request.Id, [new(first.Id, 2m), new(second.Id, 3m)]));
        await transaction.CommitAsync();
        return await db.RequestNeeds.Where(item => item.RequestId == request.Id).OrderBy(item => item.Id).ToListAsync();
    }
}
