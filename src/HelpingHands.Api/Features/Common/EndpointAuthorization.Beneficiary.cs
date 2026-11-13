using System.Security.Claims;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static bool Beneficiary(ClaimsPrincipal user) => user.IsInRole("Beneficiary");
}
