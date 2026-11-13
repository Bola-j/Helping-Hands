using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbFixture
{
    private static WebApplicationFactory<Program> CreateFactory(string connection) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:HelpingHands"] = connection,
                ["EvidenceStorage:ServiceUrl"] = "http://127.0.0.1:1",
                ["EvidenceStorage:Bucket"] = "workflow-test",
                ["EvidenceStorage:AccessKey"] = "fictional",
                ["EvidenceStorage:SecretKey"] = "fictional"
            }));
        });

    public HttpClient Client() => Factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false
    });
}
