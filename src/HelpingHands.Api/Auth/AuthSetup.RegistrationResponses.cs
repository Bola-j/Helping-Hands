namespace HelpingHands.Api.Auth;

public static partial class AuthSetup
{
    private static IResult InvalidSignup(string detail) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { ["registration"] = [detail] });

    private static IResult RolesUnavailable() =>
        Results.Problem("Development roles have not been initialized.", statusCode: 503);

    private static IResult RoleAssignmentFailed() =>
        Results.Problem("Could not assign the account role.", statusCode: 500);
}
