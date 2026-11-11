using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;
namespace HelpingHands.Api.Tests;

public sealed partial class PipelineTests
{
    [Fact]
    public async Task Login_RequiresAntiforgeryToken()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        var result = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = "test@example.test", Password = "fictional" });
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
}
