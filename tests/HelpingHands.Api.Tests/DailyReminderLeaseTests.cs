using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Reminders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task DailyReminderLease_SkipsLiveAttemptRecoversExpiredAttemptAndRejectsAddresslessSuccess()
    {
        var client = fixture.Client();
        var email = $"lease-{Guid.NewGuid():N}@example.test";
        using var registration = await WorkflowDbFixture.RegisterCharityAsync(
            client, "Lease Rep", email, fixture.Password, "Lease Charity", "lease-charity@example.test", "REG-LEASE");
        Assert.Equal(System.Net.HttpStatusCode.Created, registration.StatusCode);
        using var login = await WorkflowDbFixture.LoginRegisteredAsync(client, email, fixture.Password);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, login.StatusCode);
        var charity = await WorkflowDbFixture.RecipientAsync(client);
        var charityId = charity.GetProperty("id").GetGuid();
        await AssertLeaseRecoveryAsync(charityId);
        await AssertStoredAddressRequiredAsync(charityId);
    }

    private async Task AssertLeaseRecoveryAsync(Guid charityId)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<DailyReminderStateService>();
        var intent = await service.EnsureDailyIntentAsync(charityId, new DateOnly(2026, 11, 22));
        var first = await service.BeginDeliveryAttemptAsync(intent.Intent.IntentId, "valid@example.test");
        var active = await service.BeginDeliveryAttemptAsync(intent.Intent.IntentId, "valid@example.test");
        Assert.False(active.CanProceed);
        Assert.Equal("AttemptInProgress", active.Reason);
        var row = await database.ReminderDeliveryAttempts.SingleAsync(x => x.AttemptId == first.Attempt!.AttemptId);
        Assert.Equal("InProgress", row.Status);
        row.StartedAt = DateTimeOffset.UtcNow.AddMinutes(-6);
        await database.SaveChangesAsync();
        var recovered = await service.BeginDeliveryAttemptAsync(intent.Intent.IntentId, "valid@example.test");
        Assert.True(recovered.CanProceed);
        Assert.Equal("Failed", row.Status);
        Assert.Equal(2, recovered.Attempt!.AttemptNumber);
        await service.CompleteDeliveryAttemptAsync(recovered.Attempt.AttemptId, false);
    }

    private async Task AssertStoredAddressRequiredAsync(Guid charityId)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<DailyReminderStateService>();
        var intent = await service.EnsureDailyIntentAsync(charityId, new DateOnly(2026, 11, 22));
        var noAddress = await service.BeginDeliveryAttemptAsync(intent.Intent.IntentId, null);
        var (notDelivered, failed) = await service.CompleteDeliveryAttemptAsync(noAddress.Attempt!.AttemptId, true);
        Assert.Equal("Failed", failed.Status);
        Assert.Equal("Failed", notDelivered.Status);
        Assert.Null(notDelivered.DeliveredAt);
        var invalidAddress = await service.BeginDeliveryAttemptAsync(intent.Intent.IntentId, "not-an-email");
        var (stillNotDelivered, invalidAttempt) = await service.CompleteDeliveryAttemptAsync(invalidAddress.Attempt!.AttemptId, true);
        Assert.Equal("Failed", invalidAttempt.Status);
        Assert.Equal("Failed", stillNotDelivered.Status);
        Assert.Null(stillNotDelivered.DeliveredAt);
        var placeholder = await service.BeginDeliveryAttemptAsync(
            intent.Intent.IntentId, "charity@example.placeholder.local");
        var (placeholderIntent, placeholderAttempt) = await service.CompleteDeliveryAttemptAsync(
            placeholder.Attempt!.AttemptId, true);
        Assert.Equal("Failed", placeholderAttempt.Status);
        Assert.Equal("Failed", placeholderIntent.Status);
        Assert.Null(placeholderIntent.DeliveredAt);
    }
}
