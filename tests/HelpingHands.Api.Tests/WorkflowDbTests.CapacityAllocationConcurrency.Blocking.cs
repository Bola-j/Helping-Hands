using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private static async Task WithCharityServiceRowLockAsync(
        string connectionString, Guid charityServiceId, Func<int, Task> operation)
    {
        var blockerConnection = new NpgsqlConnectionStringBuilder(connectionString)
        { ApplicationName = "P1-53-lock-blocker" }.ConnectionString;
        await using var connection = new NpgsqlConnection(blockerConnection);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await using var pidCommand = new NpgsqlCommand("SELECT pg_backend_pid()", connection, transaction);
        var blockerPid = Convert.ToInt32(await pidCommand.ExecuteScalarAsync());
        await using var command = new NpgsqlCommand(
            "SELECT 1 FROM \"CharityService\" WHERE \"CharityServiceId\" = @serviceId FOR UPDATE", connection, transaction);
        command.Parameters.AddWithValue("serviceId", charityServiceId);
        await command.ExecuteScalarAsync();
        await operation(blockerPid);
        await transaction.CommitAsync();
    }

    private static async Task WaitForTwoServiceLockWaitersAsync(string connectionString, int blockerPid)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        for (var attempt = 0; attempt < 160; attempt++)
        {
            await using var command = new NpgsqlCommand("""
                SELECT COUNT(*) FROM pg_stat_activity waiter
                WHERE waiter.datname = current_database() AND waiter.wait_event_type = 'Lock'
                  AND waiter.query LIKE '%FOR UPDATE%'
                  AND waiter.query ILIKE '%Charity%' AND waiter.query ILIKE '%FOR UPDATE%'
                  AND (
                    @blockerPid = ANY(pg_blocking_pids(waiter.pid))
                    OR EXISTS (
                      SELECT 1 FROM pg_stat_activity direct_waiter
                      WHERE direct_waiter.pid = ANY(pg_blocking_pids(waiter.pid))
                        AND @blockerPid = ANY(pg_blocking_pids(direct_waiter.pid)))
                  )
                """, connection);
            command.Parameters.AddWithValue("blockerPid", blockerPid);
            if (Convert.ToInt32(await command.ExecuteScalarAsync()) >= 2) return;
            await Task.Delay(25);
        }
        await using var diagnostic = new NpgsqlCommand("""
            SELECT COALESCE(string_agg(pid::text || ':' || COALESCE(wait_event_type, '') || ':' ||
                COALESCE(wait_event, '') || ':' || LEFT(query, 160), ' || '), 'none')
            FROM pg_stat_activity
            WHERE datname = current_database()
              AND (pid = @blockerPid OR @blockerPid = ANY(pg_blocking_pids(pid))
                OR query ILIKE '%CharityService%')
            """, connection);
        diagnostic.Parameters.AddWithValue("blockerPid", blockerPid);
        var sessions = await diagnostic.ExecuteScalarAsync();
        throw new TimeoutException($"Did not observe two CharityService lock waiters. Sessions: {sessions}");
    }
}
