using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Reminders;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private async Task<(Guid ServiceVersionId, Guid StandardVersionId)> SeedReferralSupportAsync(Guid charityId)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var category = new ServiceCategory { Code = $"TEST-{Guid.NewGuid():N}" };
        var service = new ServiceVersion { CategoryId = category.Id, VersionNumber = 1, Name = "Fictional goods" };
        var unit = new UnitStandard { ServiceVersionId = service.Id, Name = "Fictional box" };
        var standard = new StandardVersion { UnitStandardId = unit.Id, VersionNumber = 1, EffectiveAt = DateTimeOffset.UtcNow.AddMinutes(-1) };
        database.AddRange(category, service, unit, standard,
            new StandardComponent { StandardVersionId = standard.Id, Name = "Fictional item", Quantity = 1m, Unit = "item" });
        await database.SaveChangesAsync();
        service.IsPublished = true;
        await database.SaveChangesAsync();
        standard.IsPublished = true;
        await database.SaveChangesAsync();
        await AddFreshCapacityAsync(database, charityId, service, standard);
        return (service.Id, standard.Id);
    }

    private static async Task AddFreshCapacityAsync(HelpingHandsDbContext database, Guid charityId,
        ServiceVersion service, StandardVersion standard)
    {
        var selection = new CharityService
        { CharityId = charityId, ServiceVersionId = service.Id, StandardVersionId = standard.Id };
        var now = DateTimeOffset.UtcNow;
        database.CharityServices.Add(selection);
        database.DailyCapacities.Add(new DailyCapacity
        {
            CharityServiceId = selection.CharityServiceId,
            CairoDate = DailyReminderStateService.GetCairoDate(now),
            Available = 10m,
            UpdatedAt = now
        });
        await database.SaveChangesAsync();
    }
}
