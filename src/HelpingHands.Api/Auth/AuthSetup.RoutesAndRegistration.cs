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
    public static IServiceCollection AddHelpingHandsIdentity(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddDataProtection();
        services.AddAntiforgery(ConfigureAntiforgery);
        services.AddScoped<IUserStore<AppUser>, UserStore>();
        services.AddScoped<IRoleStore<AppRole>, RoleStore>();
        services.AddIdentityCore<AppUser>(ConfigureIdentity)
            .AddRoles<AppRole>().AddSignInManager().AddDefaultTokenProviders();
        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme, ConfigureCookie);
        services.AddAuthorization();
        return services;
    }

    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder api)
    {
        var authRoutes = api.MapGroup("/auth");
        authRoutes.MapGet("/antiforgery", (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Ok(new { requestToken = tokens.RequestToken, headerName = tokens.HeaderName });
        }).AllowAnonymous();
        authRoutes.MapPost("/register", Register).AllowAnonymous().RequireRateLimiting("auth");
        authRoutes.MapPost("/login", Login).AllowAnonymous().RequireRateLimiting("auth");
        authRoutes.MapPost("/logout", Logout).RequireAuthorization();
        authRoutes.MapGet("/me", GetCurrentUser).RequireAuthorization();
        return api;
    }

    private static async Task<IResult> Register(RegisterRequest request, UserManager<AppUser> users, RoleManager<AppRole> roles, HelpingHandsDbContext database)
    {
        if (!IsValidRegistration(request))
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["registration"] = ["Provide a display name, valid email and password."] });
        if (!await roles.RoleExistsAsync("Requester"))
            return Results.Problem("Development roles have not been initialized.", statusCode: 503);
        await using var transaction = await database.Database.BeginTransactionAsync();
        var user = NewUser(request.Email.Trim(), request.DisplayName.Trim());
        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded) return RegistrationErrors(result);
        result = await users.AddToRoleAsync(user, "Requester");
        if (!result.Succeeded)
            return Results.Problem("Could not assign requester role.", statusCode: 500);
        await transaction.CommitAsync();
        return Results.Created($"/api/v1/auth/me", new { user.Id, user.DisplayName, user.Email });
    }
}
