using System.Net;
using HelpingHands.Api.Features.Reminders;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpingHands.Api.Tests;

internal static class ReminderLockOrderScenario
{
    internal sealed record Seed(Guid IntentId, Guid AttemptId);

    public static async Task RunAsync(WorkflowDbFixture fixture)
    {
        var connectionString = Environment.GetEnvironmentVariable("P1_WORKFLOW_POSTGRES")!;
        var seed = await CreateInProgressAttemptAsync(fixture);
        await using var blocker = new NpgsqlConnection(connectionString);
        await blocker.OpenAsync();
        await using var blockerTransaction = await blocker.BeginTransactionAsync();
        await ReminderLockOrderPostgres.LockIntentAsync(blocker, blockerTransaction, seed.IntentId);

        await using var beginConnection = new NpgsqlConnection(connectionString);
        await beginConnection.OpenAsync();
        await using var beginDatabase = ReminderLockOrderPostgres.CreateContext(beginConnection);
        var beginTask = new DailyReminderStateService(beginDatabase, TimeProvider.System)
            .BeginDeliveryAttemptAsync(seed.IntentId, "fictional@example.test");
        await ReminderLockOrderPostgres.WaitForLockWaitAsync(connectionString, beginConnection.ProcessID);

        await using var completionConnection = new NpgsqlConnection(connectionString);
        await completionConnection.OpenAsync();
        await using var completionDatabase = ReminderLockOrderPostgres.CreateContext(completionConnection);
        var completeTask = new DailyReminderStateService(completionDatabase, TimeProvider.System)
            .CompleteDeliveryAttemptAsync(seed.AttemptId, succeeded: true);
        await ReminderLockOrderPostgres.WaitForLockWaitAsync(connectionString, completionConnection.ProcessID);

        await blockerTransaction.CommitAsync();
        await Task.WhenAll(beginTask, completeTask).WaitAsync(TimeSpan.FromSeconds(15));
        var begun = await beginTask;
        var completed = await completeTask;
        Assert.True(begun.CanProceed);
        Assert.Equal("Failed", completed.Attempt.Status);
        Assert.Equal("Pending", completed.Intent.Status);
        await ReminderLockOrderAssertions.AssertPersistedRecoveryAsync(connectionString, seed);
    }

    private static async Task<Seed> CreateInProgressAttemptAsync(WorkflowDbFixture fixture)
    {
        using var client = fixture.Client();
        var email = $"reminder-lock-{Guid.NewGuid():N}@example.test";
        using var registration = await WorkflowDbFixture.RegisterCharityAsync(client, "Lock order", email,
            fixture.Password, "Fictional charity", "Fictional contact", "REG-LOCK");
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
        using var login = await WorkflowDbFixture.LoginRegisteredAsync(client, email, fixture.Password);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        var charity = await WorkflowDbFixture.RecipientAsync(client);
        var charityId = charity.GetProperty("id").GetGuid();
        using var scope = fixture.Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<DailyReminderStateService>();
        var intent = await service.EnsureDailyIntentAsync(charityId, new DateOnly(2026, 11, 20));
        var attempt = await service.BeginDeliveryAttemptAsync(intent.Intent.IntentId, email);
        return new Seed(intent.Intent.IntentId, attempt.Attempt!.AttemptId);
    }
}
