using System.Net;
using System.Text.Json;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Reminders;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbFixture
{
    internal sealed record LegacyReferralSetup(Guid ServiceVersionId, Guid StandardVersionId, JsonElement Accepted);

    internal static async Task<LegacyReferralSetup> PrepareLegacyReferralAsync(
        WorkflowDbFixture fixture,
        HttpClient coordinator,
        JsonElement charity,
        JsonElement accepted)
    {
        var (serviceVersionId, standardVersionId) = await SeedReferralSupportAsync(fixture, charity);
        var requestId = accepted.GetProperty("id").GetGuid();
        using var response = await PostAsync(coordinator, $"/api/v1/requests/{requestId}/legacy-standard", new
        {
            StandardVersionId = standardVersionId,
            Version = accepted.GetProperty("version").GetGuid()
        });
        var mappedRequest = await JsonAsync(response, HttpStatusCode.OK);
        return new LegacyReferralSetup(serviceVersionId, standardVersionId, mappedRequest);
    }

    internal static async Task<JsonElement> PostMappedReferralAsync(
        HttpClient coordinator,
        JsonElement charity,
        LegacyReferralSetup setup)
    {
        var requestId = setup.Accepted.GetProperty("id").GetGuid();
        using var response = await PostAsync(coordinator, $"/api/v1/requests/{requestId}/referrals", new
        {
            RecipientId = charity.GetProperty("id").GetGuid(),
            AssignedQuantity = 1m,
            ScopeDescription = "Fictional assignment",
            Version = setup.Accepted.GetProperty("version").GetGuid(),
            ServiceVersionId = setup.ServiceVersionId,
            StandardVersionId = setup.StandardVersionId
        });
        return await JsonAsync(response, HttpStatusCode.Created);
    }

    private static async Task<(Guid ServiceVersionId, Guid StandardVersionId)> SeedReferralSupportAsync(
        WorkflowDbFixture fixture,
        JsonElement charity)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var category = new ServiceCategory { Code = $"TEST-{Guid.NewGuid():N}" };
        var service = new ServiceVersion { CategoryId = category.Id, VersionNumber = 1, Name = "Fictional goods" };
        var unit = new UnitStandard { ServiceVersionId = service.Id, Name = "Fictional box" };
        var standard = new StandardVersion { UnitStandardId = unit.Id, VersionNumber = 1, EffectiveAt = DateTimeOffset.UtcNow.AddMinutes(-1) };
        database.AddRange(category, service, unit, standard, new StandardComponent
        { StandardVersionId = standard.Id, Name = "Fictional item", Quantity = 1m, Unit = "item" });
        await database.SaveChangesAsync();
        service.IsPublished = true;
        await database.SaveChangesAsync();
        standard.IsPublished = true;
        await database.SaveChangesAsync();

        var selection = new CharityService
        { CharityId = charity.GetProperty("id").GetGuid(), ServiceVersionId = service.Id, StandardVersionId = standard.Id };
        database.CharityServices.Add(selection);
        database.DailyCapacities.Add(new DailyCapacity
        {
            CharityServiceId = selection.CharityServiceId,
            CairoDate = DailyReminderStateService.GetCairoDate(DateTimeOffset.UtcNow),
            Available = 10m,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await database.SaveChangesAsync();
        return (service.Id, standard.Id);
    }
}
