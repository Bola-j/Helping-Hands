using System.Net;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbFixture
{
    public static async Task<JsonElement> RecipientAsync(HttpClient applicant)
    {
        using var response = await PostAsync(applicant, "/api/v1/recipients", new
        { DisplayName = "Fictional charity", ContactDetails = "Fictional contact", RegistrationInformation = "Fictional registration" });
        return await JsonAsync(response, HttpStatusCode.Created);
    }

    public static async Task<JsonElement> BeneficiaryAsync(HttpClient coordinator)
    {
        using var response = await PostAsync(coordinator, "/api/v1/beneficiaries", new
        { DisplayName = "Fictional beneficiary", ContactDetails = "Fictional contact", IsFamily = false });
        return await JsonAsync(response, HttpStatusCode.Created);
    }

    public static async Task<JsonElement> RequestAsync(HttpClient coordinator, Guid beneficiaryId)
    {
        using var response = await PostAsync(coordinator, "/api/v1/requests", new
        { BeneficiaryId = beneficiaryId, Description = "Fictional need", AuthorityConfirmed = true, SharingAgreed = true });
        return await JsonAsync(response, HttpStatusCode.Created);
    }
}
