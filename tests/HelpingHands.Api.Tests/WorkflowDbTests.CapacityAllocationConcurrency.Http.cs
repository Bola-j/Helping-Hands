using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private static async Task<HttpResponseMessage> PutCapacityAsync(
        HttpClient representative, CapacityAllocationScenario scenario)
    {
        using var tokenResponse = await representative.GetAsync("/api/v1/auth/antiforgery");
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
        using var request = new HttpRequestMessage(HttpMethod.Put,
            $"/api/v1/recipients/{scenario.RecipientId}/capacity")
        {
            Content = JsonContent.Create(new
            { Capacities = new[] { new { CharityServiceId = scenario.CharityServiceId, Available = 1m } } })
        };
        request.Headers.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return await representative.SendAsync(request);
    }
}
