using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task InactiveRepresentativeMembershipLosesRecordAccess()
    {
        using var representative = await fixture.LoginAsync("RecipientRepresentative");
        var charity = await WorkflowDbFixture.RecipientAsync(representative);
        var recipientId = charity.GetProperty("id").GetGuid();
        using var active = await representative.GetAsync($"/api/v1/recipients/{recipientId}");
        await fixture.SetMembershipActiveAsync(recipientId, "RecipientRepresentative", false);
        using var inactive = await representative.GetAsync($"/api/v1/recipients/{recipientId}");
        Assert.Equal(HttpStatusCode.OK, active.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, inactive.StatusCode);
    }
}
