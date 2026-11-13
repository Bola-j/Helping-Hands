using System.Data;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Needs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task NeedDatabaseRulesRejectInvalidAndLegacyWrites()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, request) = await NeedRequestAsync(scope);
        var (_, _, standard, version) = await PublishedCatalogAsync(db);
        var draft = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 2, EffectiveAt = DateTimeOffset.UtcNow };
        db.StandardVersions.Add(draft);
        await db.SaveChangesAsync();
        await RejectAsync(db, $"INSERT INTO \"RequestNeed\" VALUES ('{Guid.NewGuid()}', '{request.Id}', '{draft.Id}', 1, 'Submitted')");
        await RejectAsync(db, $"INSERT INTO \"RequestNeed\" VALUES ('{Guid.NewGuid()}', '{request.Id}', '{Guid.NewGuid()}', 1, 'Submitted')");
        await RejectAsync(db, $"UPDATE \"AssistanceRequest\" SET \"IsMultiNeed\" = FALSE WHERE \"Id\" = '{request.Id}'");
        var legacy = new AssistanceRequest { BeneficiaryId = request.BeneficiaryId, SubmittedByUserId = request.SubmittedByUserId, Reference = await db.NextReferenceAsync("REQ"), Description = "Fictional", AuthorityConfirmed = true, SharingAgreed = true };
        db.Requests.Add(legacy);
        await db.SaveChangesAsync();
        await RejectAsync(db, $"INSERT INTO \"RequestNeed\" VALUES ('{Guid.NewGuid()}', '{legacy.Id}', '{version.Id}', 1, 'Submitted')");
        var zero = new RequestNeed { RequestId = request.Id, StandardVersionId = version.Id, Quantity = 0 };
        db.RequestNeeds.Add(zero);
        Assert.Equal(PostgresErrorCodes.CheckViolation, Assert.IsType<PostgresException>((await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync())).InnerException).SqlState);
        db.ChangeTracker.Clear();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
        await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<NeedVersionService>().AddNeedsAsync(request.Id, [new(version.Id, 1)]));
    }

    [Fact]
    public async Task StandardPublicationRaceRejectsOldSelection()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, request) = await NeedRequestAsync(scope);
        var (_, _, standard, first) = await PublishedCatalogAsync(db);
        var newer = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 2, EffectiveAt = DateTimeOffset.UtcNow.AddDays(-1) };
        db.StandardVersions.Add(newer);
        await db.SaveChangesAsync();
        await using var publisher = new NpgsqlConnection(db.Database.GetConnectionString());
        await publisher.OpenAsync();
        await using var publication = await publisher.BeginTransactionAsync();
        await new NpgsqlCommand($"UPDATE \"StandardVersion\" SET \"IsPublished\" = TRUE WHERE \"Id\" = '{newer.Id}'", publisher, publication).ExecuteNonQueryAsync();
        await using var tx = await db.Database.BeginTransactionAsync();
        var attempt = scope.ServiceProvider.GetRequiredService<NeedVersionService>().AddNeedsAsync(request.Id, [new(first.Id, 1)]);
        await Task.Delay(200);
        Assert.False(attempt.IsCompleted);
        await publication.CommitAsync();
        Assert.Equal(NeedCreationResult.StaleSelection, await attempt.WaitAsync(TimeSpan.FromSeconds(10)));
    }
}
