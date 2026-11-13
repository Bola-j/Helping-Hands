using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task ClarificationUpdate_RequiresOwnerAndCurrentVersion()
    {
        using var requester = await fixture.LoginAsync("Beneficiary");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var outsider = await fixture.LoginAsync("Unrelated");
        var request = await SubmittedRequestAsync(requester);
        var clarification = await ReviewRequestAsync(coordinator, request, "Clarification", "Please clarify");
        var id = clarification.GetProperty("id").GetGuid();
        var version = clarification.GetProperty("version").GetGuid();
        using var stale = await UpdateRequestAsync(requester, id, Guid.NewGuid());
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        using var hidden = await UpdateRequestAsync(outsider, id, version);
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
        using var updated = await UpdateRequestAsync(requester, id, version);
        var result = await WorkflowDbFixture.JsonAsync(updated, HttpStatusCode.OK);
        Assert.Equal("Submitted", result.GetProperty("status").GetString());
        using var replay = await UpdateRequestAsync(requester, id, version);
        Assert.Equal(HttpStatusCode.Conflict, replay.StatusCode);
    }
}
