using System.Net;
using System.Net.Http.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class SessionCookieTests
{
    [Fact]
    public async Task ForbiddenRequest_Returns403WithoutRedirect()
    {
        using var client = fixture.Client();
        using var login = await fixture.LoginAsync(client);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        var token = await SessionCookieFixture.TokenAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/recipients/{Guid.NewGuid()}/decisions")
        {
            Content = JsonContent.Create(new { Decision = "Approved", Reason = (string?)null, Version = Guid.NewGuid() })
        };
        request.Headers.Add("X-CSRF-TOKEN", token);
        using var result = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Null(result.Headers.Location);
    }
}
