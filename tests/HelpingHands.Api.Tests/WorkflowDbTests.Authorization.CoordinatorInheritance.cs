using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task AdministratorInheritsCoordinatorRequestReview()
    {
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var administrator = await fixture.LoginAsync("Administrator");
        var first = await SelfRequestAsync(beneficiary);
        var second = await SelfRequestAsync(beneficiary);
        using var coordinatorResult = await ReviewRequestAsync(coordinator, first);
        using var administratorResult = await ReviewRequestAsync(administrator, second);
        Assert.Equal(HttpStatusCode.OK, coordinatorResult.StatusCode);
        Assert.Equal(HttpStatusCode.OK, administratorResult.StatusCode);
    }

    [Fact]
    public async Task AdministratorAndCoordinatorPassOutcomeReviewRoleGate()
    {
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var administrator = await fixture.LoginAsync("Administrator");
        var path = $"/api/v1/outcomes/{Guid.NewGuid()}/reviews";
        var input = new { Decision = "Invalid", Reason = (string?)null, Version = Guid.NewGuid() };
        using var coordinatorResult = await WorkflowDbFixture.PostAsync(coordinator, path, input);
        using var administratorResult = await WorkflowDbFixture.PostAsync(administrator, path, input);
        Assert.Equal(HttpStatusCode.BadRequest, coordinatorResult.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, administratorResult.StatusCode);
    }
}
