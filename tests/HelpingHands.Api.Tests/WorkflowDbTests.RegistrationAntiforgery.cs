using System.Net;
using System.Net.Http.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task Registration_RejectsRequestWithoutAntiforgeryToken()
    {
        using var client = fixture.Client();
        using var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        { DisplayName = "Fictional requester", Email = $"requester-{Guid.NewGuid():N}@example.test", Password = $"{Guid.NewGuid():N}Aa1!" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
