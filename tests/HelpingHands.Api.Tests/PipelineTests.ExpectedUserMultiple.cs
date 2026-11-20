using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class PipelineTests
{
    [Fact]
    public async Task ExpectedUser_RejectsMultipleValues()
    {
        using var configured = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddAuthentication("CoordinatorTest").AddScheme<AuthenticationSchemeOptions, CoordinatorTestAuthenticationHandler>("CoordinatorTest", _ => { })));
        using var client = configured.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login") { Content = JsonContent.Create(new { Email = "", Password = "" }) };
        request.Headers.Add("X-CSRF-TOKEN", await GetAntiforgeryTokenAsync(client));
        request.Headers.TryAddWithoutValidation("X-Expected-User-Id", new[] { "d3cf9e0c-f849-4e6c-9f52-204f030b7356", "d3cf9e0c-f849-4e6c-9f52-204f030b7356" });
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
