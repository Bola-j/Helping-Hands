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
    public async Task PublishedCatalogVersionsProtectTheirMeaningAndAllowNewVersions()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var category = new ServiceCategory { Code = Guid.NewGuid().ToString("N") };
        var service = new ServiceVersion { CategoryId = category.Id, VersionNumber = 1, Name = "Food" };
        var standard = new UnitStandard { ServiceVersionId = service.Id, Name = "Parcel" };
        var version = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 1, EffectiveAt = DateTimeOffset.UtcNow };
        var component = new StandardComponent { StandardVersionId = version.Id, Name = "Rice", Quantity = 1, Unit = "kg" };
        db.AddRange(category, service, standard, version, component);
        await db.SaveChangesAsync();
        await RejectAsync(db, $"UPDATE \"StandardVersion\" SET \"IsPublished\" = TRUE WHERE \"Id\" = '{version.Id}'");
        component.Quantity = 2;
        service.IsPublished = true;
        await db.SaveChangesAsync();
        version.IsPublished = true;
        await db.SaveChangesAsync();
        foreach (var sql in ProtectedWrites(category, service, standard, version, component)) await RejectAsync(db, sql);
        db.ChangeTracker.Clear();
        var next = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 2, EffectiveAt = DateTimeOffset.UtcNow };
        db.StandardVersions.Add(next);
        db.StandardComponents.Add(new StandardComponent { StandardVersionId = next.Id, Name = "Beans", Quantity = 3, Unit = "kg" });
        await db.SaveChangesAsync();
        Assert.Equal(2, (await db.StandardComponents.SingleAsync(x => x.Id == component.Id)).Quantity);
        var original = await db.StandardVersions.SingleAsync(x => x.Id == version.Id);
        original.IsPublished = false;
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private static IEnumerable<string> ProtectedWrites(ServiceCategory c, ServiceVersion s, UnitStandard u, StandardVersion v, StandardComponent item) =>
    [
        $"UPDATE \"ServiceCategory\" SET \"Code\" = 'changed' WHERE \"Id\" = '{c.Id}'",
        $"UPDATE \"ServiceVersion\" SET \"Name\" = 'changed' WHERE \"Id\" = '{s.Id}'",
        $"DELETE FROM \"ServiceVersion\" WHERE \"Id\" = '{s.Id}'",
        $"UPDATE \"UnitStandard\" SET \"Name\" = 'changed' WHERE \"Id\" = '{u.Id}'",
        $"UPDATE \"StandardVersion\" SET \"IsPublished\" = FALSE WHERE \"Id\" = '{v.Id}'",
        $"DELETE FROM \"StandardVersion\" WHERE \"Id\" = '{v.Id}'",
        $"UPDATE \"StandardComponent\" SET \"Quantity\" = 4 WHERE \"Id\" = '{item.Id}'",
        $"UPDATE \"StandardComponent\" SET \"StandardVersionId\" = '{Guid.NewGuid()}' WHERE \"Id\" = '{item.Id}'",
        $"DELETE FROM \"StandardComponent\" WHERE \"Id\" = '{item.Id}'",
        $"INSERT INTO \"StandardComponent\" (\"Id\", \"StandardVersionId\", \"Name\", \"Quantity\", \"Unit\") VALUES ('{Guid.NewGuid()}', '{v.Id}', 'Oil', 1, 'l')",
        "TRUNCATE \"StandardComponent\""
    ];

    private static async Task RejectAsync(HelpingHandsDbContext db, string sql) =>
        Assert.Equal(PostgresErrorCodes.RaiseException, (await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlRawAsync(sql))).SqlState);
}
