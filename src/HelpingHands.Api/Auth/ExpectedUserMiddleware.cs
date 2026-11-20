using System.Security.Claims;

namespace HelpingHands.Api.Auth;

public sealed class ExpectedUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        if (request.Path.StartsWithSegments("/api/v1") && request.Headers.TryGetValue("X-Expected-User-Id", out var expected))
        {
            var status = StatusFor(context, expected);
            if (status != 0)
            {
                if (status == 409) context.Response.Headers["X-Session-Changed"] = "true";
                await Results.Problem(statusCode: status, title: status == 401 ? "Sign in required" : "The signed-in account changed").ExecuteAsync(context);
                return;
            }
        }
        await next(context);
    }

    private static int StatusFor(HttpContext context, Microsoft.Extensions.Primitives.StringValues expected)
    {
        if (expected.Count != 1 || !Guid.TryParse(expected[0], out var expectedId)) return 409;
        if (context.User.Identity?.IsAuthenticated != true) return 401;
        return Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var actualId) && expectedId == actualId ? 0 : 409;
    }
}
