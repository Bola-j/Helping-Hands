using System.Net;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Needs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task MultipleNeedsEnforceDatabaseBoundariesAndBlockLegacyEndpoints()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, request) = await NeedRequestAsync(scope);
        var (_, _, _, first) = await PublishedCatalogAsync(db);
        var (_, _, _, second) = await PublishedCatalogAsync(db);
        var selector = scope.ServiceProvider.GetRequiredService<NeedVersionService>();
        await using (var tx = await db.Database.BeginTransactionAsync())
        {
            Assert.Equal(NeedCreationResult.Created, await selector.AddNeedsAsync(request.Id, [new(first.Id, 2), new(second.Id, 3)]));
            await tx.CommitAsync();
        }
        Assert.Equal(2, (await selector.GetRecordedNeedsAsync(request.Id)).Count);
        var need = await db.RequestNeeds.FirstAsync(x => x.RequestId == request.Id);
        await RejectAsync(db, $"UPDATE \"RequestNeed\" SET \"StandardVersionId\" = '{Guid.NewGuid()}' WHERE \"Id\" = '{need.Id}'");
        var duplicate = new RequestNeed { RequestId = request.Id, StandardVersionId = first.Id, Quantity = 1 };
        db.RequestNeeds.Add(duplicate);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>((await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync())).InnerException).SqlState);
        db.ChangeTracker.Clear();
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var review = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/requests/{request.Id}/decisions", new { Decision = "Accepted", Category = "Goods", RequiredQuantity = 1, Unit = "kg", Version = request.Version });
        Assert.Equal(HttpStatusCode.Conflict, review.StatusCode);
        using var referral = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/requests/{request.Id}/referrals", new { RecipientId = Guid.NewGuid(), AssignedQuantity = 1, ScopeDescription = "Fictional", Version = request.Version });
        Assert.Equal(HttpStatusCode.Conflict, referral.StatusCode);
    }

    [Fact]
    public async Task PublicationRaceRejectsOldSelectionAfterPublisherCommits()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, request) = await NeedRequestAsync(scope);
        var (category, _, _, first) = await PublishedCatalogAsync(db);
        var newer = new ServiceVersion { CategoryId = category.Id, VersionNumber = 2, Name = "New" };
        db.ServiceVersions.Add(newer);
        await db.SaveChangesAsync();
        await using var publisher = new NpgsqlConnection(db.Database.GetConnectionString());
        await publisher.OpenAsync();
        await using var publication = await publisher.BeginTransactionAsync();
        await new NpgsqlCommand($"UPDATE \"ServiceVersion\" SET \"IsPublished\" = TRUE WHERE \"Id\" = '{newer.Id}'", publisher, publication).ExecuteNonQueryAsync();
        await using var tx = await db.Database.BeginTransactionAsync();
        var attempt = scope.ServiceProvider.GetRequiredService<NeedVersionService>().AddNeedsAsync(request.Id, [new(first.Id, 1)]);
        await Task.Delay(200);
        Assert.False(attempt.IsCompleted);
        await publication.CommitAsync();
        Assert.Equal(NeedCreationResult.StaleSelection, await attempt.WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.Empty(await db.RequestNeeds.Where(x => x.RequestId == request.Id).ToListAsync());
    }
}
