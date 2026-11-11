using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public static partial class AuthSetup
{
    private static async Task<IResult> Login(LoginRequest request, UserManager<AppUser> users, SignInManager<AppUser> signIn)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
            return Results.Unauthorized();
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is null || user.AccountStatus != "Active")
            return Results.Unauthorized();
        var result = await signIn.PasswordSignInAsync(user, request.Password, isPersistent: false, lockoutOnFailure: true);
        return result.Succeeded ? Results.NoContent() : Results.Unauthorized();
    }

    public static async Task SeedDemoUsersAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (!environment.IsDevelopment())
            throw new InvalidOperationException("Demo seeding is available only in Development.");
        var (adminEmail, adminPassword, coordinatorEmail, coordinatorPassword) =
            ReadSeedSettings(scope.ServiceProvider.GetRequiredService<IConfiguration>());
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        await EnsureRoles(roles);
        await EnsureSeedUser(users, adminEmail, adminPassword, "Demo Administrator", "Administrator");
        await EnsureSeedUser(users, coordinatorEmail, coordinatorPassword, "Demo Coordinator", "Coordinator");
        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task EnsureSeedUser(UserManager<AppUser> users, string email, string password, string name, string role)
    {
        var existing = await users.FindByEmailAsync(email);
        if (existing != null)
        {
            if (!await users.IsInRoleAsync(existing, role))
                throw new InvalidOperationException("A configured demo email already belongs to a different account; no privileges were granted.");
            return;
        }
        var user = NewUser(email, name);
        Ensure(await users.CreateAsync(user, password));
        Ensure(await users.AddToRoleAsync(user, role));
    }
}
