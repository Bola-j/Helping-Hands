using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task DuplicateRegistration_EmailIsCaseInsensitive()
    {
        var email = $"duplicate-{Guid.NewGuid():N}@example.test";
        using var firstClient = fixture.Client();
        using var first = await WorkflowDbFixture.RegisterBeneficiaryAsync(firstClient, "Fictional person", email, $"{Guid.NewGuid():N}Aa1!");
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        using var secondClient = fixture.Client();
        using var second = await WorkflowDbFixture.RegisterCharityAsync(secondClient, "Fictional representative", email.ToUpperInvariant(), $"{Guid.NewGuid():N}Aa1!", "Fictional charity", "Fictional contact");
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task ConcurrentRegistration_OnlyOneAccountUsesAnEmail()
    {
        var email = $"concurrent-{Guid.NewGuid():N}@example.test";
        var attempts = Enumerable.Range(0, 2).Select(async index =>
        {
            using var client = fixture.Client();
            return index == 0
                ? await WorkflowDbFixture.RegisterBeneficiaryAsync(client, "Fictional person", email, $"{Guid.NewGuid():N}Aa1!")
                : await WorkflowDbFixture.RegisterCharityAsync(client, "Fictional representative", email, $"{Guid.NewGuid():N}Aa1!", "Fictional charity", "Fictional contact");
        });
        var results = await Task.WhenAll(attempts);
        Assert.Single(results, result => result.StatusCode == HttpStatusCode.Created);
        Assert.Single(results, result => result.StatusCode == HttpStatusCode.BadRequest);
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHands.Api.Data.HelpingHandsDbContext>();
        var userId = await database.Users.Where(user => user.NormalizedEmail == email.ToUpperInvariant()).Select(user => user.Id).SingleAsync();
        Assert.Equal(1, await database.Beneficiaries.CountAsync(item => item.LinkedUserId == userId) + await database.RecipientMembers.CountAsync(item => item.UserId == userId));
    }
}
