using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task DuplicateRegistration_EmailIsCaseInsensitive()
    {
        var email = $"duplicate-{Guid.NewGuid():N}@example.test";
        using var firstClient = fixture.Client();
        using var first = await WorkflowDbFixture.RegisterAsync(firstClient, "Fictional person", email, $"{Guid.NewGuid():N}Aa1!");
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        using var secondClient = fixture.Client();
        using var second = await WorkflowDbFixture.RegisterAsync(secondClient, "Fictional person", email.ToUpperInvariant(), $"{Guid.NewGuid():N}Aa1!");
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task ConcurrentRegistration_OnlyOneAccountUsesAnEmail()
    {
        var email = $"concurrent-{Guid.NewGuid():N}@example.test";
        var attempts = Enumerable.Range(0, 2).Select(async _ =>
        {
            using var client = fixture.Client();
            return await WorkflowDbFixture.RegisterAsync(client, "Fictional person", email, $"{Guid.NewGuid():N}Aa1!");
        });
        var results = await Task.WhenAll(attempts);
        Assert.Single(results, result => result.StatusCode == HttpStatusCode.Created);
        Assert.Single(results, result => result.StatusCode == HttpStatusCode.BadRequest);
    }
}
