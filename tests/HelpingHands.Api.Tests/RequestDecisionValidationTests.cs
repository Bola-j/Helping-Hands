using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class RequestDecisionValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public RequestDecisionValidationTests(WebApplicationFactory<Program> source)
    {
        factory = source.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:HelpingHands"] = "Host=127.0.0.1;Port=1;Database=unused;Username=unused;Timeout=1",
                ["EvidenceStorage:ServiceUrl"] = "http://127.0.0.1:1",
                ["EvidenceStorage:Bucket"] = "test-bucket",
                ["EvidenceStorage:AccessKey"] = "fictional",
                ["EvidenceStorage:SecretKey"] = "fictional"
            }));
            builder.ConfigureTestServices(services => services.AddAuthentication("CoordinatorTest")
                .AddScheme<AuthenticationSchemeOptions, CoordinatorTestAuthenticationHandler>("CoordinatorTest", _ => { }));
        });
    }
}
