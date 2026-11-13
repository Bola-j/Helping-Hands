using System.Net;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private static async Task<JsonElement> SelfRequestAsync(HttpClient beneficiary)
    {
        var profile = await WorkflowDbFixture.BeneficiaryAsync(beneficiary);
        return await WorkflowDbFixture.RequestAsync(beneficiary, profile.GetProperty("id").GetGuid());
    }

    private static async Task<HttpResponseMessage> ReviewRequestAsync(HttpClient reviewer, JsonElement request)
    {
        return await WorkflowDbFixture.PostAsync(reviewer, $"/api/v1/requests/{request.GetProperty("id").GetGuid()}/decisions",
            new { Decision = "Clarification", Reason = "Fictional clarification", Version = request.GetProperty("version").GetGuid() });
    }
}
