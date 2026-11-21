using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private static void AssertRepresentativeProgress(JsonElement progress, ProgressSeed seed)
    {
        Assert.Equal(1, progress.GetProperty("needs").GetArrayLength());
        Assert.Equal(JsonValueKind.Null, progress.GetProperty("status").ValueKind);
        Assert.Equal(JsonValueKind.Null, progress.GetProperty("description").ValueKind);
        var need = progress.GetProperty("needs")[0];
        Assert.Equal(JsonValueKind.Null, need.GetProperty("requiredQuantity").ValueKind);
        Assert.Equal(JsonValueKind.Null, need.GetProperty("status").ValueKind);
        Assert.Equal(JsonValueKind.Null, need.GetProperty("remainingQuantity").ValueKind);
        Assert.Equal(1m, need.GetProperty("assignedQuantity").GetDecimal());
        Assert.Equal(1m, need.GetProperty("acceptedQuantity").GetDecimal());
        Assert.Equal(1, need.GetProperty("assignments").GetArrayLength());
        Assert.Equal(3, need.GetProperty("assignments")[0].GetProperty("outcomes").GetArrayLength());
        AssertRepresentativeHistory(progress.GetProperty("history"), seed);
    }

    private static void AssertRepresentativeHistory(JsonElement historyElement, ProgressSeed seed)
    {
        var history = historyElement.EnumerateArray().ToArray();
        var accepted = history.Single(item => item.GetProperty("entityId").GetGuid() == seed.AcceptedOutcomeId);
        var clarification = history.Single(item => item.GetProperty("entityId").GetGuid() == seed.ClarificationOutcomeId);
        var unresolved = history.Single(item => item.GetProperty("entityId").GetGuid() == seed.UnresolvedOutcomeId);
        Assert.Equal(JsonValueKind.Null, accepted.GetProperty("reason").ValueKind);
        Assert.Equal("Please clarify the receipt", clarification.GetProperty("reason").GetString());
        Assert.Equal(JsonValueKind.Null, unresolved.GetProperty("reason").ValueKind);
        Assert.All(history, item => Assert.Equal(JsonValueKind.Null, item.GetProperty("actorUserId").ValueKind));
        Assert.DoesNotContain(history, item => item.GetProperty("entityId").GetGuid() == seed.SecondReferralId);
    }

    private static void AssertCoordinatorProgress(JsonElement progress, ProgressSeed seed)
    {
        Assert.Equal(2, progress.GetProperty("needs").GetArrayLength());
        var history = progress.GetProperty("history").EnumerateArray().ToArray();
        Assert.Equal("Internal acceptance note", history.Single(item =>
            item.GetProperty("entityId").GetGuid() == seed.AcceptedOutcomeId).GetProperty("reason").GetString());
        Assert.Equal("Internal unresolved note", history.Single(item =>
            item.GetProperty("entityId").GetGuid() == seed.UnresolvedOutcomeId).GetProperty("reason").GetString());
        Assert.All(history.Where(item => item.GetProperty("entityType").GetString() == "Outcome"),
            item => Assert.NotEqual(JsonValueKind.Null, item.GetProperty("actorUserId").ValueKind));
    }
}
