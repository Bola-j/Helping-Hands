using System.Net;
using System.Text.Json;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Needs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task<AcceptedNeedAllocationScenario> PrepareAcceptedNeedAllocationScenarioAsync()
    {
        using var representative = await fixture.LoginAsync("RecipientRepresentative");
        using var admin = await fixture.LoginAsync("Administrator");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        var (charity, accepted) = await WorkflowDbFixture.AcceptedNeedAsync(
            representative, admin, coordinator, beneficiary);
        var capacity = await WorkflowDbFixture.PrepareLegacyReferralAsync(fixture, coordinator, charity, accepted);
        var secondStandardVersionId = await CreateAdditionalPublishedVersionAsync();
        var needs = await CreateMultiNeedRequestAsync(capacity.StandardVersionId, secondStandardVersionId);
        await ReviewNeedsAsync(coordinator, needs);
        return new(needs.RequestId, needs.AcceptedNeedId, needs.RejectedNeedId, charity.GetProperty("id").GetGuid(),
            capacity.ServiceVersionId, capacity.StandardVersionId, fixture.UserIds["Coordinator"]);
    }

    private async Task<Guid> CreateAdditionalPublishedVersionAsync()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (_, _, _, version) = await PublishedCatalogAsync(
            scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>());
        return version.Id;
    }

    private async Task<MultiNeedAllocationScenario> CreateMultiNeedRequestAsync(
        Guid acceptedStandardVersionId, Guid rejectedStandardVersionId)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (database, request) = await NeedRequestAsync(scope);
        await using (var transaction = await database.Database.BeginTransactionAsync())
        {
            var versions = scope.ServiceProvider.GetRequiredService<NeedVersionService>();
            Assert.Equal(NeedCreationResult.Created,
                await versions.AddNeedsAsync(request.Id,
                    [new(acceptedStandardVersionId, 1m), new(rejectedStandardVersionId, 1m)]));
            await transaction.CommitAsync();
        }
        var needs = await database.RequestNeeds.Where(item => item.RequestId == request.Id).ToListAsync();
        return new(request.Id, needs.Single(item => item.StandardVersionId == acceptedStandardVersionId).Id,
            needs.Single(item => item.StandardVersionId == rejectedStandardVersionId).Id, request.Version);
    }
}

internal sealed record AcceptedNeedAllocationScenario(
    Guid RequestId, Guid NeedId, Guid RejectedNeedId, Guid RecipientId, Guid ServiceVersionId,
    Guid StandardVersionId, Guid ActorUserId);

internal sealed record MultiNeedAllocationScenario(
    Guid RequestId, Guid AcceptedNeedId, Guid RejectedNeedId, Guid RequestVersion);
