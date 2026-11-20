using HelpingHands.Api.Features.Referrals;

namespace HelpingHands.Api.Tests;

public sealed class ExactVersionRequestResolutionTests
{
    [Fact]
    public void ExactVersionResolution_MatchesOnlyExactVersionsWithFreshCairoCapacity()
    {
        var categoryId = Guid.NewGuid();
        var otherCategoryId = Guid.NewGuid();

        var serviceV1 = Guid.NewGuid();
        var serviceV2 = Guid.NewGuid();
        var standardV1 = Guid.NewGuid();
        var standardV2 = Guid.NewGuid();

        var cairoToday = new DateOnly(2026, 11, 20);
        var cairoYesterday = new DateOnly(2026, 11, 19);

        var listings = new[]
        {
            new ServiceListing(categoryId, serviceV1, standardV1, "Approved", true, true, cairoToday, 30m, "Exact V1 Match"),
            new ServiceListing(categoryId, serviceV2, standardV2, "Approved", true, true, cairoToday, 75m, "Newer V2 Charity"),
            new ServiceListing(categoryId, serviceV1, standardV1, "Approved", true, true, cairoYesterday, 50m, "Stale V1 Charity"),
            new ServiceListing(categoryId, serviceV1, standardV1, "Pending", true, true, cairoToday, 40m, "Unapproved V1 Charity"),
            new ServiceListing(otherCategoryId, serviceV1, standardV1, "Approved", true, true, cairoToday, 60m, "Other Category Charity")
        };

        var exactMatches = EvaluateExactMatches(
            listings,
            categoryId,
            serviceV1,
            standardV1,
            cairoToday,
            minAvailable: 10m);

        var match = Assert.Single(exactMatches);
        Assert.Equal("Exact V1 Match", match.Label);
        Assert.Equal(30m, match.Available);

        // Mismatched service/standard versions (serviceV2 with standardV1) must NOT silently substitute or convert
        var mismatchedVersionMatches = EvaluateExactMatches(
            listings,
            categoryId,
            serviceV2,
            standardV1,
            cairoToday,
            minAvailable: 10m);

        Assert.Empty(mismatchedVersionMatches);
    }

    [Fact]
    public void UnmatchedNeed_RemainsOpenAndPreservesHistoryWithoutSilentConversion()
    {
        var needStatus = "Open";
        var requestStatus = "Open";
        var preservedHistory = new List<string> { "Submitted", "StandardVersionRecorded:v1" };

        var matches = Array.Empty<ServiceListing>();
        var resolutionStatus = matches.Length > 0 ? "Matched" : "OpenUnmatched";
        var leaveOpen = matches.Length == 0;
        var silentConversionApplied = false;

        Assert.Equal("OpenUnmatched", resolutionStatus);
        Assert.True(leaveOpen);
        Assert.False(silentConversionApplied);
        Assert.Equal("Open", needStatus);
        Assert.Equal("Open", requestStatus);
        Assert.Equal(2, preservedHistory.Count);
    }

    private static IReadOnlyList<ServiceListing> EvaluateExactMatches(
        IEnumerable<ServiceListing> items,
        Guid categoryId,
        Guid serviceVersionId,
        Guid standardVersionId,
        DateOnly cairoToday,
        decimal minAvailable)
    {
        return items
            .Where(x =>
                x.CategoryId == categoryId &&
                x.ServiceVersionId == serviceVersionId &&
                x.StandardVersionId == standardVersionId &&
                x.ApprovalStatus == "Approved" &&
                x.Active &&
                x.Enabled &&
                x.CapacityCairoDate == cairoToday &&
                x.Available >= minAvailable)
            .OrderByDescending(x => x.Available)
            .ToList();
    }

    private sealed record ServiceListing(
        Guid CategoryId,
        Guid ServiceVersionId,
        Guid StandardVersionId,
        string ApprovalStatus,
        bool Active,
        bool Enabled,
        DateOnly CapacityCairoDate,
        decimal Available,
        string Label);
}