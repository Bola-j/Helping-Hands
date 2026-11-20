using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HelpingHands.Api.Tests;

internal static class ReminderLockOrderAssertions
{
    public static async Task AssertPersistedRecoveryAsync(string connectionString, ReminderLockOrderScenario.Seed seed)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var database = new HelpingHandsDbContext(
            new DbContextOptionsBuilder<HelpingHandsDbContext>().UseNpgsql(connection).Options);
        var intent = await database.DailyReminderIntents.SingleAsync(row => row.IntentId == seed.IntentId);
        var attempts = await database.ReminderDeliveryAttempts
            .Where(row => row.IntentId == seed.IntentId).OrderBy(row => row.AttemptNumber).ToListAsync();
        Assert.Equal("Pending", intent.Status);
        Assert.Null(intent.DeliveredAt);
        Assert.Equal(2, intent.AttemptCount);
        Assert.Equal(new[] { "Failed", "InProgress" }, attempts.Select(row => row.Status));
        Assert.Equal(seed.AttemptId, attempts[0].AttemptId);
        Assert.NotNull(attempts[0].CompletedAt);
    }
}
