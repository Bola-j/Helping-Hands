using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class PipelineTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("invalid-fictional-token")]
    public async Task MissingOrInvalidAntiforgeryToken_IsRejected(string? token)
    {
        using var client = AntiforgeryClient();
        await GetAntiforgeryTokenAsync(client);
        using var response = await LoginWithTokenAsync(client, token);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task TokenWithoutItsCookie_IsRejected()
    {
        using var donor = AntiforgeryClient();
        using var recipient = AntiforgeryClient();
        var token = await GetAntiforgeryTokenAsync(donor);
        using var response = await LoginWithTokenAsync(recipient, token);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TokenFromDifferentCookieSession_IsRejected()
    {
        using var donor = AntiforgeryClient();
        using var recipient = AntiforgeryClient();
        var token = await GetAntiforgeryTokenAsync(donor);
        await GetAntiforgeryTokenAsync(recipient);
        using var response = await LoginWithTokenAsync(recipient, token);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
