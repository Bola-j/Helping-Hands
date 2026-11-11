using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;

namespace HelpingHands.Api.Auth;

public static partial class AuthSetup
{
    private static void ConfigureAntiforgery(AntiforgeryOptions options)
    {
        options.HeaderName = "X-CSRF-TOKEN";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
    }

    private static void ConfigureIdentity(IdentityOptions options)
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 12;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    }

    private static void ConfigureCookie(CookieAuthenticationOptions options)
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context => RejectRedirect(context, 401),
            OnRedirectToAccessDenied = context => RejectRedirect(context, 403),
            OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
        };
    }
}
