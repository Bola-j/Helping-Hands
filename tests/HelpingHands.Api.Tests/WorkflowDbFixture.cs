using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public string Password { get; } = $"{Guid.NewGuid():N}Aa1!";
    public Dictionary<string, string> Emails { get; } = new();
    public Dictionary<string, Guid> UserIds { get; } = new();
    private Dictionary<string, string> SessionCookies { get; } = new();

    public async Task InitializeAsync()
    {
        var connection = Environment.GetEnvironmentVariable("P1_WORKFLOW_POSTGRES");
        if (string.IsNullOrWhiteSpace(connection) ||
            !(new NpgsqlConnectionStringBuilder(connection).Database ?? "").StartsWith("helping_hands_workflow", StringComparison.Ordinal))
            throw new InvalidOperationException("P1_WORKFLOW_POSTGRES must reference an isolated helping_hands_workflow database.");
        Factory = CreateFactory(connection);
        using var scope = Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>().Database.MigrateAsync();
        await SeedUsersAsync(scope.ServiceProvider);
    }

    public Task DisposeAsync()
    {
        Factory?.Dispose();
        return Task.CompletedTask;
    }
}
