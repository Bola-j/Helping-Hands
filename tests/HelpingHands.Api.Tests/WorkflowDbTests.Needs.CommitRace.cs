using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Needs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task NeedCreationHoldsPublicationLockUntilTransactionCommits()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, request) = await NeedRequestAsync(scope);
        var (category, _, _, version) = await PublishedCatalogAsync(db);
        var newer = new ServiceVersion { CategoryId = category.Id, VersionNumber = 2, Name = "New" };
        db.ServiceVersions.Add(newer);
        await db.SaveChangesAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var selector = scope.ServiceProvider.GetRequiredService<NeedVersionService>();
        Assert.Equal(NeedCreationResult.Created, await selector.AddNeedsAsync(request.Id, [new(version.Id, 1)]));
        await using var publisher = new NpgsqlConnection(db.Database.GetConnectionString());
        await publisher.OpenAsync();
        var publish = new NpgsqlCommand($"UPDATE \"ServiceVersion\" SET \"IsPublished\" = TRUE WHERE \"Id\" = '{newer.Id}'", publisher).ExecuteNonQueryAsync();
        await Task.Delay(200);
        Assert.False(publish.IsCompleted);
        await transaction.CommitAsync();
        Assert.Equal(1, await publish.WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.Equal(version.Id, Assert.Single(await selector.GetRecordedNeedsAsync(request.Id)).StandardVersionId);
    }
}
