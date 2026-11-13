using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private static async Task<JsonElement> SubmittedRequestAsync(HttpClient client)
    {
        var beneficiary = await WorkflowDbFixture.BeneficiaryAsync(client);
        return await WorkflowDbFixture.RequestAsync(client, beneficiary.GetProperty("id").GetGuid());
    }

    private static async Task<JsonElement> ReviewRequestAsync(HttpClient client, JsonElement request, string decision, string reason)
    {
        using var response = await WorkflowDbFixture.PostAsync(client, $"/api/v1/requests/{request.GetProperty("id").GetGuid()}/decisions",
            new { Decision = decision, Reason = reason, Version = request.GetProperty("version").GetGuid() });
        return await WorkflowDbFixture.JsonAsync(response, HttpStatusCode.OK);
    }

    private static async Task<HttpResponseMessage> UpdateRequestAsync(HttpClient client, Guid id, Guid version)
    {
        using var tokenResponse = await client.GetAsync("/api/v1/auth/antiforgery");
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/requests/{id}")
        { Content = JsonContent.Create(new { Description = "Fictional clarification", Version = version }) };
        request.Headers.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return await client.SendAsync(request);
    }
}
