namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbFixture
{
    public static void AssertNoAuthenticationCookie(HttpResponseMessage response)
    {
        var hasAuthenticationCookie = response.Headers.TryGetValues("Set-Cookie", out var cookies) &&
            cookies.Any(cookie => cookie.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
        Assert.False(hasAuthenticationCookie);
    }
}
