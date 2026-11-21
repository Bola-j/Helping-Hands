using HelpingHands.Api.Features.Referrals;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task<CapacityAllocationOutcome> RunWritersAfterProvenContentionAsync(
        CapacityAllocationRun run)
    {
        var connectionString = Environment.GetEnvironmentVariable("P1_WORKFLOW_POSTGRES")
            ?? throw new InvalidOperationException("P1_WORKFLOW_POSTGRES is required.");
        Task<SplitReferralAllocationResult> allocation = null!;
        Task<HttpResponseMessage> capacityUpdate = null!;
        await WithCharityServiceRowLockAsync(connectionString, run.Scenario.CharityServiceId, async blockerPid =>
        {
            var allocator = run.AllocationScope.ServiceProvider.GetRequiredService<SplitReferralAllocationService>();
            allocation = allocator.AllocateSplitAsync(new(run.Scenario.RequestId, null,
                fixture.UserIds["Coordinator"], [new(run.Scenario.RecipientId, run.Scenario.ServiceVersionId,
                    run.Scenario.StandardVersionId, 1m, "Fictional concurrent allocation")]));
            capacityUpdate = PutCapacityAsync(run.Representative, run.Scenario);
            await WaitForTwoServiceLockWaitersAsync(connectionString, blockerPid);
        });
        await Task.WhenAll(allocation, capacityUpdate);
        return new(await capacityUpdate, await allocation);
    }
}

internal sealed record CapacityAllocationOutcome(
    HttpResponseMessage CapacityResponse, SplitReferralAllocationResult Allocation);
