using HelpingHands.Api.Auth;
using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public string Administrator { get; } = $"admin-{Guid.NewGuid():N}@example.test";
    public string Coordinator { get; } = $"coordinator-{Guid.NewGuid():N}@example.test";
    public string Representative { get; } = $"charity-{Guid.NewGuid():N}@example.test";
    public string Outsider { get; } = $"outsider-{Guid.NewGuid():N}@example.test";
    public string Password { get; } = $"{Guid.NewGuid():N}Aa1!";

    public async Task InitializeAsync()
    {
        var connection = Required("P1_E2E_POSTGRES");
        var database = new NpgsqlConnectionStringBuilder(connection).Database;
        if (database is null || !database.StartsWith("helping_hands_p108_e2e", StringComparison.Ordinal))
            throw new InvalidOperationException("P1_E2E_POSTGRES must identify a dedicated helping_hands_p108_e2e database.");
        Factory = CreateFactory(connection);
        using var scope = Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>().Database.MigrateAsync();
        await AuthSetup.SeedDemoUsersAsync(Factory.Services);
    }

    public Task DisposeAsync()
    {
        Factory?.Dispose();
        return Task.CompletedTask;
    }
}
