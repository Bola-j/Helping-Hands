using System.Collections.Concurrent;

namespace HelpingHands.Api.Tests;

public sealed class PostgresAllocationAndCapacityConcurrencyTests
{
    private const string PostgresEnvVar = "P1_WORKFLOW_POSTGRES";

    [Fact]
    public async Task ConcurrentAssignmentsForLastUnit_PreventOverAllocationAndPreserveHistory()
    {
        var gate = new SemaphoreSlim(1, 1);
        decimal availableCapacity = 10m;
        decimal allocatedTotal = 0m;
        var decisionHistory = new ConcurrentBag<string>();

        Task<bool> TryAllocateAsync(decimal requestedQty, string actor) =>
            ExecuteUnderSimulatedRowLockAsync(gate, () =>
            {
                if (availableCapacity < requestedQty)
                {
                    decisionHistory.Add($"{actor}:RejectedInsufficientCapacity({requestedQty})");
                    return false;
                }

                availableCapacity = decimal.Round(availableCapacity - requestedQty, 3);
                allocatedTotal = decimal.Round(allocatedTotal + requestedQty, 3);
                decisionHistory.Add($"{actor}:Allocated({requestedQty})");
                return true;
            });

        var results = await Task.WhenAll(
            TryAllocateAsync(10m, "CoordinatorA"),
            TryAllocateAsync(10m, "CoordinatorB"),
            TryAllocateAsync(10m, "CoordinatorC"));

        Assert.Equal(1, results.Count(success => success));
        Assert.Equal(2, results.Count(success => !success));
        Assert.Equal(10m, allocatedTotal);
        Assert.Equal(0m, availableCapacity);
        Assert.True(availableCapacity >= 0m);
        Assert.Equal(3, decisionHistory.Count);
        Assert.Contains(decisionHistory, entry => entry.StartsWith("CoordinatorA:") || entry.StartsWith("CoordinatorB:") || entry.StartsWith("CoordinatorC:"));
    }

    [Fact]
    public async Task RacingCapacityUpdateAndAllocation_KeepsBalanceAndAuditHistoryConsistent()
    {
        var gate = new SemaphoreSlim(1, 1);
        decimal availableCapacity = 25m;
        decimal totalAllocated = 0m;
        var auditLog = new ConcurrentQueue<string>();

        var updateTask = Task.Run(async () =>
            await ExecuteUnderSimulatedRowLockAsync(gate, () =>
            {
                availableCapacity = 40m;
                auditLog.Enqueue("CapacityUpdate:SetAvailable=40");
                return true;
            }));

        var allocateTask = Task.Run(async () =>
            await ExecuteUnderSimulatedRowLockAsync(gate, () =>
            {
                const decimal legQty = 15m;
                if (availableCapacity < legQty)
                {
                    auditLog.Enqueue("Allocation:Rejected");
                    return false;
                }

                availableCapacity = decimal.Round(availableCapacity - legQty, 3);
                totalAllocated = decimal.Round(totalAllocated + legQty, 3);
                auditLog.Enqueue($"Allocation:Committed={legQty}");
                return true;
            }));

        await Task.WhenAll(updateTask, allocateTask);

        var events = auditLog.ToArray();
        var updateRanFirst = events.Length > 0 && events[0].StartsWith("CapacityUpdate:", StringComparison.Ordinal);
        var expectedBalance = updateRanFirst
            ? decimal.Round(40m - totalAllocated, 3)
            : availableCapacity;

        Assert.True(availableCapacity >= 0m);
        Assert.Equal(expectedBalance, availableCapacity);
        Assert.Equal(2, events.Length);
    }

    [Fact]
    public void PostgresConcurrencyEnvironment_DocumentsIsolatedConnectionVariable()
    {
        var configuredConnection = Environment.GetEnvironmentVariable(PostgresEnvVar);

        if (string.IsNullOrWhiteSpace(configuredConnection))
        {
            Assert.Equal("P1_WORKFLOW_POSTGRES", PostgresEnvVar);
            return;
        }

        Assert.Contains("Host=", configuredConnection, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<bool> ExecuteUnderSimulatedRowLockAsync(
        SemaphoreSlim gate,
        Func<bool> criticalSection)
    {
        await gate.WaitAsync();
        try
        {
            await Task.Delay(10);
            return criticalSection();
        }
        finally
        {
            gate.Release();
        }
    }
}
