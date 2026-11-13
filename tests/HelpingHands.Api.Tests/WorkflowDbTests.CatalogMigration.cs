using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task PublicationWaitsForComponentWriteAndThenRejectsFurtherWrites()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var c = new ServiceCategory { Code = Guid.NewGuid().ToString("N") };
        var s = new ServiceVersion { CategoryId = c.Id, VersionNumber = 1, Name = "Food" };
        var u = new UnitStandard { ServiceVersionId = s.Id, Name = "Parcel" };
        var v = new StandardVersion { UnitStandardId = u.Id, VersionNumber = 1, EffectiveAt = DateTimeOffset.UtcNow };
        db.AddRange(c, s, u, v);
        await db.SaveChangesAsync();
        s.IsPublished = true;
        await db.SaveChangesAsync();
        var connectionString = db.Database.GetConnectionString()!;
        await using var first = new NpgsqlConnection(connectionString);
        await using var second = new NpgsqlConnection(connectionString);
        await first.OpenAsync();
        await second.OpenAsync();
        await using var transaction = await first.BeginTransactionAsync();
        await new NpgsqlCommand($"INSERT INTO \"StandardComponent\" VALUES ('{Guid.NewGuid()}', '{v.Id}', 'Rice', 1, 'kg')", first, transaction).ExecuteNonQueryAsync();
        var publish = new NpgsqlCommand($"UPDATE \"StandardVersion\" SET \"IsPublished\" = TRUE WHERE \"Id\" = '{v.Id}'", second).ExecuteNonQueryAsync();
        await Task.Delay(200);
        Assert.False(publish.IsCompleted);
        await transaction.CommitAsync();
        await publish.WaitAsync(TimeSpan.FromSeconds(10));
        await RejectAsync(db, $"INSERT INTO \"StandardComponent\" VALUES ('{Guid.NewGuid()}', '{v.Id}', 'Oil', 1, 'l')");
        Assert.Equal(1, await db.StandardComponents.CountAsync(x => x.StandardVersionId == v.Id));
    }

    [Fact]
    public async Task UpgradePreservesAndProtectsExistingCatalogData()
    {
        var baseConnection = Environment.GetEnvironmentVariable("P1_WORKFLOW_POSTGRES")!;
        var builder = new NpgsqlConnectionStringBuilder(baseConnection);
        var dbName = "helping_hands_workflow_upgrade_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(baseConnection);
        await admin.OpenAsync();
        await new NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", admin).ExecuteNonQueryAsync();
        builder.Database = dbName;
        builder.Pooling = false;
        try { await VerifyUpgradeAsync(builder.ConnectionString); }
        finally { await new NpgsqlCommand($"DROP DATABASE \"{dbName}\"", admin).ExecuteNonQueryAsync(); }
    }

    private static async Task VerifyUpgradeAsync(string connection)
    {
        var options = new DbContextOptionsBuilder<HelpingHandsDbContext>().UseNpgsql(connection).Options;
        await using var db = new HelpingHandsDbContext(options);
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync("20261113110151_ServiceCatalog");
        var category = Guid.NewGuid(); var service = Guid.NewGuid(); var standard = Guid.NewGuid(); var version = Guid.NewGuid(); var component = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "ServiceCategory" VALUES ({category}, 'FOOD');
            INSERT INTO "ServiceVersion" VALUES ({service}, {category}, 1, 'Food');
            INSERT INTO "UnitStandard" VALUES ({standard}, {service}, 'Parcel');
            INSERT INTO "StandardVersion" VALUES ({version}, {standard}, 1, '2026-01-01T00:00:00Z');
            INSERT INTO "StandardComponent" VALUES ({component}, {version}, 'Rice', 1.25, 'kg');
            """);
        await migrator.MigrateAsync();
        Assert.True((await db.ServiceVersions.SingleAsync(x => x.Id == service)).IsPublished);
        Assert.True((await db.StandardVersions.SingleAsync(x => x.Id == version)).IsPublished);
        var saved = await db.StandardComponents.SingleAsync(x => x.Id == component);
        Assert.Equal(version, saved.StandardVersionId);
        Assert.Equal(1.25m, saved.Quantity);
        await RejectAsync(db, $"DELETE FROM \"StandardComponent\" WHERE \"Id\" = '{component}'");
        await migrator.MigrateAsync("20261113110151_ServiceCatalog");
        Assert.Equal(1L, await db.Database.SqlQueryRaw<long>("SELECT count(*) AS \"Value\" FROM \"StandardComponent\"").SingleAsync());
    }
}

