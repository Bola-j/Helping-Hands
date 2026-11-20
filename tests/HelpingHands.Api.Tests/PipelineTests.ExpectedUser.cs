using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class PipelineTests
{
    [Theory]
    [InlineData("adfd4137-b731-4723-a479-85d0a0191556")]
    [InlineData("not-a-guid")]
    public async Task ExpectedUser_RejectsDifferentOrInvalidAccount(string expected)
    {
        using var configured = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddAuthentication("CoordinatorTest").AddScheme<AuthenticationSchemeOptions, CoordinatorTestAuthenticationHandler>("CoordinatorTest", _ => { })));
        using var client = configured.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        using var response = await LoginWithExpectedUser(client, expected);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("true", Assert.Single(response.Headers.GetValues("X-Session-Changed")));
        Assert.DoesNotContain(expected, await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("d3cf9e0c-f849-4e6c-9f52-204f030b7356")]
    public async Task ExpectedUser_AbsentOrMatchingAccountPreservesHandler(string? expected)
    {
        using var configured = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddAuthentication("CoordinatorTest").AddScheme<AuthenticationSchemeOptions, CoordinatorTestAuthenticationHandler>("CoordinatorTest", _ => { })));
        using var client = configured.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        using var response = await LoginWithExpectedUser(client, expected);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(response.Headers.Contains("X-Session-Changed"));
    }

    private static async Task<HttpResponseMessage> LoginWithExpectedUser(HttpClient client, string? expected)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login") { Content = JsonContent.Create(new { Email = "", Password = "" }) };
        request.Headers.Add("X-CSRF-TOKEN", await GetAntiforgeryTokenAsync(client));
        if (expected is not null) request.Headers.TryAddWithoutValidation("X-Expected-User-Id", expected);
        return await client.SendAsync(request);
    }
}
