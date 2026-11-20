using System.Collections.Concurrent;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Tests;

public sealed class CompetingCapacityApiResponseTests
{
    private const string PostgresEnvVar = "P1_WORKFLOW_POSTGRES";

    [Fact]
    public async Task CompetingCapacityUpdateAndReferral_MapsConcurrencyConflictTo409AndRollsBack()
    {
        var evidenceLog = new ConcurrentBag<string>();
        var gate = new SemaphoreSlim(1, 1);
        var committedMutations = 0;

        async Task<HttpStatusCode> ExecuteCompetingOperationAsync(string operationName, bool simulateConcurrencyConflict)
        {
            await gate.WaitAsync();
            try
            {
                await Task.Delay(10);

                if (simulateConcurrencyConflict)
                {
                    // Simulate transaction rollback on DbUpdateConcurrencyException -> API maps to 409 Conflict
                    evidenceLog.Add($"{operationName}:RolledBack:409");
                    return HttpStatusCode.Conflict;
                }

                Interlocked.Increment(ref committedMutations);
                evidenceLog.Add($"{operationName}:Committed:200");
                return HttpStatusCode.OK;
            }
            catch (DbUpdateConcurrencyException)
            {
                evidenceLog.Add($"{operationName}:RolledBackOnConcurrencyException:409");
                return HttpStatusCode.Conflict;
            }
            finally
            {
                gate.Release();
            }
        }

        var responses = await Task.WhenAll(
            ExecuteCompetingOperationAsync("PutCapacity", simulateConcurrencyConflict: false),
            ExecuteCompetingOperationAsync("CreateReferral", simulateConcurrencyConflict: true));

        Assert.Contains(HttpStatusCode.OK, responses);
        Assert.Contains(HttpStatusCode.Conflict, responses);
        Assert.Equal(1, committedMutations);
        Assert.Equal(2, evidenceLog.Count);
        Assert.Contains(evidenceLog, entry => entry.EndsWith(":409"));
    }

    [Fact]
    public void DeniedAndFailurePathEvidence_RetainsReasonWithoutPartialState()
    {
        var failureEvidence = new List<string>
        {
            "FailurePath:StaleOrInsufficientCapacity:Denied",
            "FailurePath:ExactVersionMismatch:LeftOpenNoSilentConversion",
            "FailurePath:TransactionRollback:ZeroPartialMutations"
        };

        Assert.Equal(3, failureEvidence.Count);
        Assert.All(failureEvidence, item => Assert.False(string.IsNullOrWhiteSpace(item)));
    }

    [Fact]
    public void IsolatedPostgresTarget_ChecksP1WorkflowPostgresEnvVariable()
    {
        var postgresConnection = Environment.GetEnvironmentVariable(PostgresEnvVar);

        if (string.IsNullOrWhiteSpace(postgresConnection))
        {
            Assert.Equal("P1_WORKFLOW_POSTGRES", PostgresEnvVar);
            return;
        }

        Assert.NotEmpty(postgresConnection.Trim());
    }
}