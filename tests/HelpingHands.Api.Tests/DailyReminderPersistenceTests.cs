using System.Net;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Reminders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task DailyReminderPersistence_EnforcesOneIntentPerCairoDay_SupportsRestartAndRetry_AndPreventsDuplicateDelivery()
    {
        var repClient = fixture.Client();
        var repEmail = $"rep-rem-{Guid.NewGuid():N}@example.test";
        var regRep = await WorkflowDbFixture.RegisterCharityAsync(
            repClient, "Rep Reminder", repEmail, fixture.Password, "Reminder Charity", "rem@example.test", "REG-P124");
        Assert.Equal(HttpStatusCode.Created, regRep.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await WorkflowDbFixture.LoginRegisteredAsync(repClient, repEmail, fixture.Password)).StatusCode);

        var charity = await WorkflowDbFixture.RecipientAsync(repClient);
        var charityId = charity.GetProperty("id").GetGuid();

        var cairoDay1 = new DateOnly(2026, 11, 20);
        var cairoDay2 = new DateOnly(2026, 11, 21);

        Guid intentId;

        // 1. Idempotent intent creation per charity + Cairo day
        using (var scope1 = fixture.Factory.Services.CreateScope())
        {
            var service1 = scope1.ServiceProvider.GetRequiredService<DailyReminderStateService>();
            var first = await service1.EnsureDailyIntentAsync(charityId, cairoDay1);
            Assert.True(first.Created);
            Assert.Equal("Pending", first.Intent.Status);
            Assert.Null(first.Intent.DeliveredAt);

            var duplicateCall = await service1.EnsureDailyIntentAsync(charityId, cairoDay1);
            Assert.False(duplicateCall.Created);
            Assert.Equal(first.Intent.IntentId, duplicateCall.Intent.IntentId);
            intentId = first.Intent.IntentId;

            // Start Attempt 1 -> InProgress (does NOT claim delivery)
            var begin1 = await service1.BeginDeliveryAttemptAsync(intentId, repEmail);
            Assert.True(begin1.CanProceed);
            Assert.NotNull(begin1.Attempt);
            Assert.Equal(1, begin1.Attempt!.AttemptNumber);
            Assert.Equal("InProgress", begin1.Attempt.Status);
            Assert.Equal("Pending", begin1.Intent.Status);
            Assert.Null(begin1.Intent.DeliveredAt);
        }

        using (var recoveryScope = fixture.Factory.Services.CreateScope())
        {
            var database = recoveryScope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
            var firstAttempt = await database.ReminderDeliveryAttempts.SingleAsync(row => row.IntentId == intentId);
            firstAttempt.StartedAt = DateTimeOffset.UtcNow.AddMinutes(-6);
            await database.SaveChangesAsync();
        }

        // 2. Simulate process restart / new DI scope while Attempt 1 was still InProgress:
        // EnsureDailyIntent returns same intent without claiming delivery, and BeginDeliveryAttempt recovers Attempt 1 as Failed before starting Attempt 2
        using (var scopeAfterRestart = fixture.Factory.Services.CreateScope())
        {
            var serviceAfterRestart = scopeAfterRestart.ServiceProvider.GetRequiredService<DailyReminderStateService>();
            var afterRestartIntent = await serviceAfterRestart.EnsureDailyIntentAsync(charityId, cairoDay1);
            Assert.False(afterRestartIntent.Created);
            Assert.Equal(intentId, afterRestartIntent.Intent.IntentId);
            Assert.Equal("Pending", afterRestartIntent.Intent.Status);
            Assert.Null(afterRestartIntent.Intent.DeliveredAt);

            var begin2 = await serviceAfterRestart.BeginDeliveryAttemptAsync(intentId, repEmail);
            Assert.True(begin2.CanProceed);
            Assert.NotNull(begin2.Attempt);
            Assert.Equal(2, begin2.Attempt!.AttemptNumber);
            Assert.Equal("InProgress", begin2.Attempt.Status);

            // Simulate Attempt 2 failing due to SMTP error
            var (failedIntent, failedAttempt2) = await serviceAfterRestart.CompleteDeliveryAttemptAsync(
                begin2.Attempt.AttemptId, succeeded: false, failureReason: "SMTP connection refused");
            Assert.Equal("Failed", failedAttempt2.Status);
            Assert.Equal("SMTP connection refused", failedAttempt2.FailureReason);
            Assert.Equal("Failed", failedIntent.Status);
            Assert.Null(failedIntent.DeliveredAt);
        }

        // 3. Retry after failure -> Attempt 3 succeeds and records Delivered
        using (var retryScope = fixture.Factory.Services.CreateScope())
        {
            var retryService = retryScope.ServiceProvider.GetRequiredService<DailyReminderStateService>();
            var begin3 = await retryService.BeginDeliveryAttemptAsync(intentId, repEmail);
            Assert.True(begin3.CanProceed);
            Assert.NotNull(begin3.Attempt);
            Assert.Equal(3, begin3.Attempt!.AttemptNumber);

            var (deliveredIntent, succeededAttempt3) = await retryService.CompleteDeliveryAttemptAsync(
                begin3.Attempt.AttemptId, succeeded: true);
            Assert.Equal("Succeeded", succeededAttempt3.Status);
            Assert.Equal("Delivered", deliveredIntent.Status);
            Assert.NotNull(deliveredIntent.DeliveredAt);

            // 4. Subsequent restart/retry on same Cairo day must NOT duplicate recorded success
            var reEnsure = await retryService.EnsureDailyIntentAsync(charityId, cairoDay1);
            Assert.False(reEnsure.Created);
            Assert.Equal("Delivered", reEnsure.Intent.Status);

            var duplicateBegin = await retryService.BeginDeliveryAttemptAsync(intentId, repEmail);
            Assert.False(duplicateBegin.CanProceed);
            Assert.Equal("AlreadyDelivered", duplicateBegin.Reason);
            Assert.Null(duplicateBegin.Attempt);
            Assert.Equal(3, duplicateBegin.Intent.AttemptCount);

            // 5. Next Cairo day allows a separate independent reminder intent
            var nextDayIntent = await retryService.EnsureDailyIntentAsync(charityId, cairoDay2);
            Assert.True(nextDayIntent.Created);
            Assert.NotEqual(intentId, nextDayIntent.Intent.IntentId);
            Assert.Equal("Pending", nextDayIntent.Intent.Status);
        }

        // 6. Verify persisted state and attempt history via API endpoint
        using var apiRes = await repClient.GetAsync($"/api/v1/recipients/{charityId}/reminders?cairoDate=2026-11-20");
        var body = await WorkflowDbFixture.JsonAsync(apiRes, HttpStatusCode.OK);
        var intents = body.GetProperty("intents").EnumerateArray().ToList();
        var singleIntent = Assert.Single(intents);
        Assert.Equal("Delivered", singleIntent.GetProperty("status").GetString());
        Assert.Equal(3, singleIntent.GetProperty("attemptCount").GetInt32());

        var attempts = singleIntent.GetProperty("attempts").EnumerateArray().ToList();
        Assert.Equal(3, attempts.Count);
        Assert.Equal("Failed", attempts[0].GetProperty("status").GetString());
        Assert.Equal("Failed", attempts[1].GetProperty("status").GetString());
        Assert.Equal("Succeeded", attempts[2].GetProperty("status").GetString());
    }
}
