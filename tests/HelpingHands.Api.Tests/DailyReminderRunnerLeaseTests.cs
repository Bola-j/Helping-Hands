using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Reminders;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task ReminderRunner_CountsLiveAttemptSeparatelyFromDeliveredSkip()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var charityId = await CreateStaleApprovedCharityAsync(database);
        var intentId = await SeedLiveReminderAttemptAsync(database, charityId);
        var runner = scope.ServiceProvider.GetRequiredService<CairoCapacityReminderRunner>();
        var summary = await runner.RunDueRemindersAsync(forceRunNow: true);
        Assert.True(summary.SkippedInProgressCount >= 1);
        var attempt = await database.ReminderDeliveryAttempts.SingleAsync(row => row.IntentId == intentId);
        Assert.Equal("InProgress", attempt.Status);
    }

    private async Task<Guid> CreateStaleApprovedCharityAsync(HelpingHandsDbContext database)
    {
        var (_, service, _, standard) = await PublishedCatalogAsync(database);
        using var representative = fixture.Client();
        var email = $"stale-{Guid.NewGuid():N}@example.test";
        using var registration = await WorkflowDbFixture.RegisterCharityAsync(
            representative, "Reminder representative", email, fixture.Password,
            "Reminder charity", "contact@example.test", $"REG-{Guid.NewGuid():N}");
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
        using var login = await WorkflowDbFixture.LoginRegisteredAsync(representative, email, fixture.Password);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        var charity = await WorkflowDbFixture.RecipientAsync(representative);
        using var administrator = await fixture.LoginAsync("Administrator");
        await WorkflowDbFixture.ApproveAsync(administrator, charity);
        var charityId = charity.GetProperty("id").GetGuid();
        database.CharityServices.Add(new CharityService
        { CharityId = charityId, ServiceVersionId = service.Id, StandardVersionId = standard.Id });
        await database.SaveChangesAsync();
        return charityId;
    }

    private async Task<Guid> SeedLiveReminderAttemptAsync(HelpingHandsDbContext database, Guid charityId)
    {
        var date = DailyReminderStateService.GetCairoDate(DateTimeOffset.UtcNow);
        var intent = new DailyReminderIntent { CharityId = charityId, CairoDate = date, ScheduledForUtc = DateTimeOffset.UtcNow };
        database.DailyReminderIntents.Add(intent);
        database.ReminderDeliveryAttempts.Add(new ReminderDeliveryAttempt
        {
            IntentId = intent.IntentId, AttemptNumber = 1, RecipientEmail = "valid@example.test",
            Status = "InProgress", StartedAt = DateTimeOffset.UtcNow
        });
        await database.SaveChangesAsync();
        return intent.IntentId;
    }
}
