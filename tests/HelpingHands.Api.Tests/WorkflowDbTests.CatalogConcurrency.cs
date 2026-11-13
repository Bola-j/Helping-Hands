using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task ConcurrentWritesCannotChangeVersionAfterPublicationCommits()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var c = new ServiceCategory { Code = Guid.NewGuid().ToString("N") };
        var s = new ServiceVersion { CategoryId = c.Id, VersionNumber = 1, Name = "Food" };
        var u = new UnitStandard { ServiceVersionId = s.Id, Name = "Parcel" };
        var v = new StandardVersion { UnitStandardId = u.Id, VersionNumber = 1, EffectiveAt = DateTimeOffset.UtcNow };
        db.AddRange(c, s, u, v);
        await db.SaveChangesAsync();
        await using var publisher = new NpgsqlConnection(db.Database.GetConnectionString());
        await using var writer = new NpgsqlConnection(db.Database.GetConnectionString());
        await publisher.OpenAsync();
        await writer.OpenAsync();
        await using var tx = await publisher.BeginTransactionAsync();
        await new NpgsqlCommand($"UPDATE \"ServiceVersion\" SET \"IsPublished\" = TRUE WHERE \"Id\" = '{s.Id}'", publisher, tx).ExecuteNonQueryAsync();
        await new NpgsqlCommand($"UPDATE \"StandardVersion\" SET \"IsPublished\" = TRUE WHERE \"Id\" = '{v.Id}'", publisher, tx).ExecuteNonQueryAsync();
        var change = new NpgsqlCommand($"UPDATE \"UnitStandard\" SET \"Name\" = 'changed' WHERE \"Id\" = '{u.Id}'", writer).ExecuteNonQueryAsync();
        await Task.Delay(200);
        Assert.False(change.IsCompleted);
        await tx.CommitAsync();
        Assert.Equal(PostgresErrorCodes.RaiseException, (await Assert.ThrowsAsync<PostgresException>(() => change)).SqlState);
        Assert.Equal("Parcel", (await db.UnitStandards.SingleAsync(x => x.Id == u.Id)).Name);
    }
}
