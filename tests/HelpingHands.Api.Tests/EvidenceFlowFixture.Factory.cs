using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowFixture
{
    private static string Required(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
            ? value : throw new InvalidOperationException($"{name} is required for isolated evidence integration tests.");

    private WebApplicationFactory<Program> CreateFactory(string connection) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:HelpingHands"] = connection,
                ["EvidenceStorage:ServiceUrl"] = Required("P1_TEST_S3_URL"),
                ["EvidenceStorage:Bucket"] = Required("P1_TEST_S3_BUCKET"),
                ["EvidenceStorage:AccessKey"] = Required("P1_TEST_S3_ACCESS_KEY"),
                ["EvidenceStorage:SecretKey"] = Required("P1_TEST_S3_SECRET_KEY"),
                ["DemoSeed:AdminEmail"] = Administrator, ["DemoSeed:AdminPassword"] = Password,
                ["DemoSeed:CoordinatorEmail"] = Coordinator, ["DemoSeed:CoordinatorPassword"] = Password
            }));
        });

    public HttpClient Client() => Factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false
    });
}
