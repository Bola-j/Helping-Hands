using HelpingHands.Api.Features.Referrals;

namespace HelpingHands.Api.Tests;

public sealed class PostgresAllocationLockProtocolTests
{
    [Fact]
    public void OrderedLockTiers_FollowCanonicalHierarchy()
    {
        var tiers = PostgresAllocationLockProtocol.OrderedLockTiers;

        Assert.Equal(4, tiers.Count);
        Assert.StartsWith("1. AssistanceRequest", tiers[0]);
        Assert.StartsWith("2. ReferralRecipient / Charity", tiers[1]);
        Assert.StartsWith("3. CharityService", tiers[2]);
        Assert.StartsWith("4. DailyCapacity", tiers[3]);
    }

    [Fact]
    public void OrderIdsDeterministically_DeduplicatesAndSortsAscending()
    {
        var idC = Guid.Parse("cccccccc-0000-0000-0000-000000000003");
        var idA = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
        var idB = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");

        var ordered = PostgresAllocationLockProtocol.OrderIdsDeterministically([idC, idA, idB, idA, idC]);

        Assert.Equal([idA, idB, idC], ordered);
    }
}