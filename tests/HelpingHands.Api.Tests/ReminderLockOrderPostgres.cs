using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HelpingHands.Api.Tests;

internal static class ReminderLockOrderPostgres
{
    public static HelpingHandsDbContext CreateContext(NpgsqlConnection connection) =>
        new(new DbContextOptionsBuilder<HelpingHandsDbContext>().UseNpgsql(connection).Options);

    public static async Task LockIntentAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid intentId)
    {
        await using var command = new NpgsqlCommand(
            "SELECT 1 FROM \"DailyReminderIntent\" WHERE \"IntentId\" = @intentId FOR UPDATE", connection, transaction);
        command.Parameters.AddWithValue("intentId", intentId);
        await command.ExecuteScalarAsync();
    }

    public static async Task WaitForLockWaitAsync(string connectionString, int processId)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        var expires = DateTimeOffset.UtcNow.AddSeconds(10);
        while (DateTimeOffset.UtcNow < expires)
        {
            await using var command = new NpgsqlCommand(
                "SELECT wait_event_type FROM pg_stat_activity WHERE pid = @pid", connection);
            command.Parameters.AddWithValue("pid", processId);
            if (await command.ExecuteScalarAsync() as string == "Lock")
                return;
            await Task.Delay(10);
        }
        throw new TimeoutException($"PostgreSQL backend {processId} did not enter a lock wait.");
    }
}
