using System.Net;
using System.Net.Http.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task BothRegistrationRoutes_RejectRequestWithoutAntiforgeryToken()
    {
        using var client = fixture.Client();
        using var beneficiary = await client.PostAsJsonAsync("/api/v1/auth/register/beneficiary", new
        { DisplayName = "Fictional beneficiary", Email = $"beneficiary-{Guid.NewGuid():N}@example.test", Password = $"{Guid.NewGuid():N}Aa1!", ContactDetails = "Fictional contact", IsFamily = false });
        using var charity = await client.PostAsJsonAsync("/api/v1/auth/register/charity", new
        { RepresentativeDisplayName = "Fictional representative", Email = $"charity-{Guid.NewGuid():N}@example.test", Password = $"{Guid.NewGuid():N}Aa1!", OrganizationName = "Fictional charity", ContactDetails = "Fictional contact" });
        Assert.Equal(HttpStatusCode.BadRequest, beneficiary.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, charity.StatusCode);
    }
}
