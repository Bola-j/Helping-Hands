using System.Net;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbFixture
{
    public static async Task<JsonElement> ApproveAsync(HttpClient admin, JsonElement recipient)
    {
        var id = recipient.GetProperty("id").GetGuid();
        using var response = await PostAsync(admin, $"/api/v1/recipients/{id}/decisions", new
        { Decision = "Approved", Reason = (string?)null, Version = recipient.GetProperty("version").GetGuid() });
        return await JsonAsync(response, HttpStatusCode.OK);
    }

    public static async Task<JsonElement> AcceptAsync(HttpClient coordinator, JsonElement request)
    {
        var id = request.GetProperty("id").GetGuid();
        using var response = await PostAsync(coordinator, $"/api/v1/requests/{id}/decisions", new
        { Decision = "Accepted", Category = "Goods", RequiredQuantity = 1m, Unit = "box", Version = request.GetProperty("version").GetGuid() });
        return await JsonAsync(response, HttpStatusCode.OK);
    }
    public static async Task<JsonElement> AcceptedRequestAsync(HttpClient beneficiary, HttpClient coordinator)
    {
        var profile = await BeneficiaryAsync(beneficiary);
        var request = await RequestAsync(beneficiary, profile.GetProperty("id").GetGuid());
        return await AcceptAsync(coordinator, request);
    }
}
