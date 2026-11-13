using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public WorkflowValidationTests(WebApplicationFactory<Program> source)
    {
        factory = source.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(Settings()));
            builder.ConfigureTestServices(services => services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { }));
        });
    }

    [Fact]
    public async Task RequestWithoutAuthority_IsRejectedBeforeDatabaseAccess()
    {
        using var client = Client();
        var response = await PostWithToken(client, "/api/v1/requests", new
        {
            BeneficiaryId = Guid.NewGuid(),
            Description = "Fictional request",
            AuthorityConfirmed = false,
            SharingAgreed = true
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BeneficiaryCannotApproveCharity()
    {
        using var client = Client();
        var response = await PostWithToken(client, $"/api/v1/recipients/{Guid.NewGuid()}/decisions", new
        {
            Decision = "Approved",
            Reason = (string?)null,
            Version = Guid.NewGuid()
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
