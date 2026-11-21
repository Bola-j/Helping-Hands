using System.Reflection;
using HelpingHands.Api.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IEvidenceStorage>();
                services.AddSingleton<S3EvidenceStorage>();
                services.AddSingleton<IEvidenceStorage>(provider =>
                {
                    var proxy = DispatchProxy.Create<IEvidenceStorage, EvidenceStorageRecordingProxy>();
                    ((EvidenceStorageRecordingProxy)(object)proxy).Inner = provider.GetRequiredService<S3EvidenceStorage>();
                    return proxy;
                });
            });
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
