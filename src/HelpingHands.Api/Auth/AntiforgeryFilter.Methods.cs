namespace HelpingHands.Api.Auth;

public sealed partial class AntiforgeryFilter
{
    private static bool RequiresValidation(string method) =>
        !HttpMethods.IsGet(method) && !HttpMethods.IsHead(method) &&
        !HttpMethods.IsOptions(method) && !HttpMethods.IsTrace(method);
}
