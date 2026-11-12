using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class SessionCookieFixture : IAsyncLifetime
{
    public SessionTestClock Clock { get; } = new();
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public string Email { get; } = $"session-{Guid.NewGuid():N}@example.test";
    public string Password { get; } = $"{Guid.NewGuid():N}Aa1!";

    public async Task InitializeAsync()
    {
        var connection = Environment.GetEnvironmentVariable("P108_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("P108_TEST_POSTGRES is required for isolated session tests.");
        var database = new NpgsqlConnectionStringBuilder(connection).Database;
        if (database is null || !database.StartsWith("helping_hands_p108", StringComparison.Ordinal))
            throw new InvalidOperationException("P108_TEST_POSTGRES must identify a dedicated helping_hands_p108 test database.");
        Factory = CreateFactory(connection);
        using var scope = Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>().Database.MigrateAsync();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = new AppUser { UserName = Email, Email = Email, DisplayName = "Fictional session user" };
        var result = await users.CreateAsync(user, Password);
        if (!result.Succeeded) throw new InvalidOperationException("Failed to create fictional session user.");
    }

    public Task DisposeAsync()
    {
        Factory?.Dispose();
        return Task.CompletedTask;
    }
}
