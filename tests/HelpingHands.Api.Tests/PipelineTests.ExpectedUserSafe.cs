using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class PipelineTests
{
    [Theory]
    [InlineData(null, HttpStatusCode.OK)]
    [InlineData("d3cf9e0c-f849-4e6c-9f52-204f030b7356", HttpStatusCode.OK)]
    [InlineData("adfd4137-b731-4723-a479-85d0a0191556", HttpStatusCode.Conflict)]
    [InlineData("not-a-guid", HttpStatusCode.Conflict)]
    public async Task ExpectedUser_GuardsAuthenticatedReads(string? expected, HttpStatusCode status)
    {
        using var configured = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddAuthentication("CoordinatorTest").AddScheme<AuthenticationSchemeOptions, CoordinatorTestAuthenticationHandler>("CoordinatorTest", _ => { })));
        using var client = configured.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/antiforgery");
        if (expected is not null) request.Headers.TryAddWithoutValidation("X-Expected-User-Id", expected);
        using var response = await client.SendAsync(request);
        Assert.Equal(status, response.StatusCode);
        Assert.Equal(status == HttpStatusCode.Conflict, response.Headers.Contains("X-Session-Changed"));
    }
}
