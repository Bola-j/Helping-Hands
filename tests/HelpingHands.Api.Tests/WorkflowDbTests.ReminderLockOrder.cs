namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public Task ReminderRecoveryAndCompletion_UseOnePostgresLockOrder() =>
        ReminderLockOrderScenario.RunAsync(fixture);
}
