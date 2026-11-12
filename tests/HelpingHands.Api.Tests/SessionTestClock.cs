namespace HelpingHands.Api.Tests;

public sealed class SessionTestClock : TimeProvider
{
    private DateTimeOffset now = DateTimeOffset.UtcNow;

    public override DateTimeOffset GetUtcNow() => now;

    public void Advance(TimeSpan duration) => now = now.Add(duration);
}
