using System.Security.Claims;
using HelpingHands.Api.Features.Referrals;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    public sealed record SplitReferralInput(Guid Version, IReadOnlyList<SplitAllocationLegInput> Allocations);

    private static async Task<IResult> AllocateNeedReferrals(
        Guid requestId, Guid needId, SplitReferralInput input, ClaimsPrincipal user,
        SplitReferralAllocationService allocator, CancellationToken cancellationToken)
    {
        if (!Coordinator(user)) return Results.Forbid();
        var command = new SplitReferralAllocationRequest(requestId, needId, Actor(user), input.Allocations, input.Version);
        var result = await allocator.AllocateSplitAsync(command, cancellationToken);
        return result.Succeeded ? Results.Created("/api/v1/requests/" + requestId + "/needs/" + needId + "/referrals", result) : AllocationFailure(result);
    }

    private static IResult AllocationFailure(SplitReferralAllocationResult result) => result.ErrorCode switch
    {
        "RequestNotFound" or "NeedNotFound" => Results.NotFound(new { result.ErrorCode, result.ErrorMessage }),
        "InvalidInput" or "InvalidQuantity" or "InvalidScope" => Results.BadRequest(new { result.ErrorCode, result.ErrorMessage }),
        _ => Results.Conflict(new { result.ErrorCode, result.ErrorMessage, result.LeaveOpen })
    };
}
