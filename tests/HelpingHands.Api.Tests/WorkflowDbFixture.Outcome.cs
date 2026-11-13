using System.Net;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbFixture
{
    public static async Task<JsonElement> CreateOutcomeAsync(HttpClient representative, JsonElement referral)
    {
        var response = await PostAsync(representative, $"/api/v1/referrals/{referral.GetProperty("id").GetGuid()}/outcomes", new
        { ReportedQuantity = 1m, Description = "Fictional report" });
        return await JsonAsync(response, HttpStatusCode.Created);
    }
}
