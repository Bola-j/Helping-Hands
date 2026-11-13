using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task Catalog_PersistsVersionsAndEnforcesRelationalRules()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var (category, service, standard, version) = await SeedCatalogAsync(db);
        db.ChangeTracker.Clear();
        Assert.Equal(2, await db.ServiceVersions.CountAsync(x => x.CategoryId == category.Id));
        Assert.Equal(2, await db.UnitStandards.CountAsync(x => x.ServiceVersionId == service.Id));
        Assert.Equal(2, await db.StandardVersions.CountAsync(x => x.UnitStandardId == standard.Id));
        Assert.Equal(1.250m, (await db.StandardComponents.SingleAsync(x => x.StandardVersionId == version.Id)).Quantity);
        await AssertCatalogConstraintsAsync(db, category, standard, version);
    }

    private static async Task<(ServiceCategory, ServiceVersion, UnitStandard, StandardVersion)> SeedCatalogAsync(HelpingHandsDbContext db)
    {
        var category = new ServiceCategory { Code = Guid.NewGuid().ToString("N") };
        var service = new ServiceVersion { CategoryId = category.Id, VersionNumber = 1, Name = "Food" };
        var standard = new UnitStandard { ServiceVersionId = service.Id, Name = "Family parcel" };
        var version = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 1, EffectiveAt = DateTimeOffset.UtcNow };
        db.AddRange(category, service, standard, version,
            new ServiceVersion { CategoryId = category.Id, VersionNumber = 2, Name = "Food revised" },
            new UnitStandard { ServiceVersionId = service.Id, Name = "Individual parcel" },
            new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 2, EffectiveAt = DateTimeOffset.UtcNow },
            new StandardComponent { StandardVersionId = version.Id, Name = "Rice", Quantity = 1.250m, Unit = "kg" });
        await db.SaveChangesAsync();
        return (category, service, standard, version);
    }

    private static async Task AssertCatalogConstraintsAsync(HelpingHandsDbContext db, ServiceCategory category, UnitStandard standard, StandardVersion version)
    {
        var duplicateService = new ServiceVersion { CategoryId = category.Id, VersionNumber = 1, Name = "Duplicate" };
        var duplicateStandard = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 1, EffectiveAt = DateTimeOffset.UtcNow };
        await AssertInsertFailsAsync(db, duplicateService, PostgresErrorCodes.UniqueViolation);
        await AssertInsertFailsAsync(db, duplicateStandard, PostgresErrorCodes.UniqueViolation);
        await AssertInsertFailsAsync(db, new ServiceVersion { CategoryId = category.Id, VersionNumber = 0, Name = "Invalid" }, PostgresErrorCodes.CheckViolation);
        await AssertInsertFailsAsync(db, new UnitStandard { ServiceVersionId = Guid.NewGuid(), Name = "Missing parent" }, PostgresErrorCodes.ForeignKeyViolation);
        await AssertInsertFailsAsync(db, new StandardComponent { StandardVersionId = version.Id, Name = "Rice", Quantity = 0, Unit = "kg" }, PostgresErrorCodes.CheckViolation);
        var error = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"ServiceCategory\" WHERE \"Id\" = {category.Id}"));
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, error.SqlState);
    }
}
