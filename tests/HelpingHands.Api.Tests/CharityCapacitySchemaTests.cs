using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HelpingHands.Api.Tests;

public sealed class CharityCapacitySchemaTests
{
    private static IModel CreateDesignModel()
    {
        using var db = new HelpingHandsDbContext(
            new DbContextOptionsBuilder<HelpingHandsDbContext>()
                .UseNpgsql("Host=localhost;Database=unused")
                .Options);
        return db.GetService<IDesignTimeModel>().Model;
    }

    [Fact]
    public void CharityCapacityEntities_ConfigureExpectedKeysAndUniqueIndexes()
    {
        var model = CreateDesignModel();
        var serviceEntity = model.FindEntityType(typeof(CharityService))!;
        var capacityEntity = model.FindEntityType(typeof(DailyCapacity))!;
        var updateEntity = model.FindEntityType(typeof(CapacityUpdate))!;

        Assert.Equal(["CharityServiceId"], serviceEntity.FindPrimaryKey()!.Properties.Select(p => p.Name));
        Assert.Contains(serviceEntity.GetIndexes(), i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(["CharityId", "ServiceVersionId", "StandardVersionId"]));
        Assert.Contains(capacityEntity.GetIndexes(), i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(["CharityServiceId", "CairoDate"]));
        Assert.NotNull(updateEntity.FindPrimaryKey());
    }

    [Fact]
    public void CharityCapacityEntities_ConfigureNonNegativeCheckConstraints()
    {
        var model = CreateDesignModel();
        var capacityChecks = model.FindEntityType(typeof(DailyCapacity))!.GetCheckConstraints().Select(c => c.Name);
        var updateChecks = model.FindEntityType(typeof(CapacityUpdate))!.GetCheckConstraints().Select(c => c.Name);

        Assert.Contains("CK_DailyCapacity_Available_NonNegative", capacityChecks);
        Assert.Contains("CK_CapacityUpdate_Available_NonNegative", updateChecks);
    }
}