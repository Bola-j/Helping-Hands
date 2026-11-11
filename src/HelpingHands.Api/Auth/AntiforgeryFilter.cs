using Microsoft.AspNetCore.Antiforgery;

namespace HelpingHands.Api.Auth;

public sealed partial class AntiforgeryFilter(IAntiforgery antiforgery) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!RequiresValidation(context.HttpContext.Request.Method))
            return await next(context);
        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.Problem("Invalid or missing antiforgery token.", statusCode: StatusCodes.Status400BadRequest);
        }
        return await next(context);
    }
}
