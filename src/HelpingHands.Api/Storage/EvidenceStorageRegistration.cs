using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Options;

namespace HelpingHands.Api.Storage;

public static class EvidenceStorageRegistration
{
    public static IServiceCollection AddEvidenceStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EvidenceStorageOptions>()
            .Bind(configuration.GetSection(EvidenceStorageOptions.SectionName))
            .Validate(HasRequiredSettings, "Evidence storage settings are incomplete.")
            .ValidateOnStart();
        services.AddSingleton<IAmazonS3>(CreateClient);
        services.AddSingleton<IEvidenceStorage, S3EvidenceStorage>();
        return services;
    }

    private static bool HasRequiredSettings(EvidenceStorageOptions options) =>
        Uri.TryCreate(options.ServiceUrl, UriKind.Absolute, out _) &&
        !string.IsNullOrWhiteSpace(options.Bucket) &&
        !string.IsNullOrWhiteSpace(options.AccessKey) &&
        !string.IsNullOrWhiteSpace(options.SecretKey);

    private static IAmazonS3 CreateClient(IServiceProvider services)
    {
        var settings = services.GetRequiredService<IOptions<EvidenceStorageOptions>>().Value;
        var credentials = new BasicAWSCredentials(settings.AccessKey, settings.SecretKey);
        var configuration = new AmazonS3Config
        {
            ServiceURL = settings.ServiceUrl,
            ForcePathStyle = true,
            AuthenticationRegion = "us-east-1"
        };
        return new AmazonS3Client(credentials, configuration);
    }
}
