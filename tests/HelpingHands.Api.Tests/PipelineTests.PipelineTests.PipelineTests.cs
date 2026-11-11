using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;
namespace HelpingHands.Api.Tests;

public sealed partial class PipelineTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public PipelineTests(WebApplicationFactory<Program> source)
    {
        factory = source.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:HelpingHands"] = "Host=127.0.0.1;Port=1;Database=unused;Username=unused;Timeout=1",
                ["EvidenceStorage:ServiceUrl"] = "http://127.0.0.1:1",
                ["EvidenceStorage:Bucket"] = "test-bucket",
                ["EvidenceStorage:AccessKey"] = "fictional",
                ["EvidenceStorage:SecretKey"] = "fictional"
            }));
        });
    }

    [Fact]
    public async Task Liveness_DoesNotRequireDatabase()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        var result = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_RejectsAnonymousWithoutRedirect()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        var result = await client.GetAsync("/api/v1/requests");
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
}
