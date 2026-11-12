using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class SessionCookieFixture
{
    private WebApplicationFactory<Program> CreateFactory(string connection) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:HelpingHands"] = connection,
                ["EvidenceStorage:ServiceUrl"] = "http://127.0.0.1:1",
                ["EvidenceStorage:Bucket"] = "test-bucket",
                ["EvidenceStorage:AccessKey"] = "fictional",
                ["EvidenceStorage:SecretKey"] = "fictional"
            }));
            builder.ConfigureTestServices(services => services.Configure<CookieAuthenticationOptions>(
                IdentityConstants.ApplicationScheme, options => options.TimeProvider = Clock));
        });

    public HttpClient Client() => Factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });
}
