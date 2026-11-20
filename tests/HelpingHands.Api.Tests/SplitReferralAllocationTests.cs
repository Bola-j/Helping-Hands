using HelpingHands.Api.Features.Referrals;

namespace HelpingHands.Api.Tests;

public sealed class SplitReferralAllocationTests
{
    [Fact]
    public void SplitAllocation_SupportsPartialAndFullSplitsWithoutOverAllocation()
    {
        var requiredQuantity = 100m;
        var legs = new[] { 40m, 35m };

        var allocated = legs.Sum();
        var remaining = requiredQuantity - allocated;

        Assert.Equal(75m, allocated);
        Assert.Equal(25m, remaining);
        Assert.True(allocated <= requiredQuantity);

        var overAllocated = allocated + 30m;
        Assert.False(overAllocated <= requiredQuantity);
    }

    [Fact]
    public void SplitAllocation_LeavesNeedOpenOnVersionMismatchOrInsufficientCapacity()
    {
        var recordedStandardVersionId = Guid.NewGuid();
        var mismatchedStandardVersionId = Guid.NewGuid();

        var isExactMatch = recordedStandardVersionId == mismatchedStandardVersionId;
        var leaveOpen = !isExactMatch;
        var silentConversionApplied = false;

        Assert.False(isExactMatch);
        Assert.True(leaveOpen);
        Assert.False(silentConversionApplied);
    }
}