using Npgsql;

namespace HelpingHands.Api.Tests;

internal sealed class PostgresFailingInsertTrigger(string connectionString, string tableName) : IAsyncDisposable
{
    private readonly string triggerName = $"p153_fail_{Guid.NewGuid():N}";
    private readonly string functionName = $"p153_raise_{Guid.NewGuid():N}";

    public static async Task<PostgresFailingInsertTrigger> InstallAsync(string connectionString, string tableName)
    {
        if (tableName is not ("CapacityUpdate" or "DecisionHistory"))
            throw new ArgumentOutOfRangeException(nameof(tableName));

        var trigger = new PostgresFailingInsertTrigger(connectionString, tableName);
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(trigger.CreateSql(), connection);
        await command.ExecuteNonQueryAsync();
        return trigger;
    }

    public async ValueTask DisposeAsync()
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            $"DROP TRIGGER IF EXISTS \"{triggerName}\" ON \"{tableName}\"; DROP FUNCTION IF EXISTS \"{functionName}\"();",
            connection);
        await command.ExecuteNonQueryAsync();
    }

    private string CreateSql() => $"""
        CREATE FUNCTION "{functionName}"() RETURNS trigger LANGUAGE plpgsql AS $$
        BEGIN
            RAISE EXCEPTION 'P1-53 intentional persistence failure';
            RETURN NEW;
        END;
        $$;
        CREATE TRIGGER "{triggerName}" BEFORE INSERT ON "{tableName}"
        FOR EACH ROW EXECUTE FUNCTION "{functionName}"();
        """;
}
