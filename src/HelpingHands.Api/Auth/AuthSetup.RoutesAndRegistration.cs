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
        authRoutes.MapPost("/register/beneficiary", RegisterBeneficiary).AllowAnonymous().RequireRateLimiting("auth");
        authRoutes.MapPost("/register/charity", RegisterCharity).AllowAnonymous().RequireRateLimiting("auth");
        authRoutes.MapPost("/login", Login).AllowAnonymous().RequireRateLimiting("auth");
        authRoutes.MapPost("/logout", Logout).RequireAuthorization();
        authRoutes.MapGet("/me", GetCurrentUser).RequireAuthorization();
        return api;
    }

}
