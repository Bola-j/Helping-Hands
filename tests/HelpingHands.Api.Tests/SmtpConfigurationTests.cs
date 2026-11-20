using HelpingHands.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HelpingHands.Api.Tests;

public sealed class SmtpConfigurationTests
{
    [Fact]
    public void SmtpOptions_BindsFromEnvironmentStyleConfiguration()
    {
        var values = new Dictionary<string, string?>
        {
            ["Smtp:Host"] = "127.0.0.1",
            ["Smtp:Port"] = "1025",
            ["Smtp:Username"] = "fictional-user",
            ["Smtp:Password"] = "fictional-pass",
            ["Smtp:FromAddress"] = "reminders@helping-hands.example.test",
            ["Smtp:UseSsl"] = "false"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        var services = new ServiceCollection();
        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName))
            .ValidateDataAnnotations();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<SmtpOptions>>().Value;

        Assert.Equal("127.0.0.1", options.Host);
        Assert.Equal(1025, options.Port);
        Assert.Equal("fictional-user", options.Username);
        Assert.Equal("fictional-pass", options.Password);
        Assert.Equal("reminders@helping-hands.example.test", options.FromAddress);
        Assert.False(options.UseSsl);
    }

    [Fact]
    public void ComposeAndEnvExample_ConfigureIsolatedLoopbackSmtpCatcherWithoutHardcodedSecrets()
    {
        var repoRoot = FindRepositoryRoot();
        var composeText = File.ReadAllText(Path.Combine(repoRoot, "compose.yaml"));
        var envExampleText = File.ReadAllText(Path.Combine(repoRoot, ".env.example"));

        Assert.Contains("mailpit:", composeText);
        Assert.Contains("image: axllent/mailpit:", composeText);
        Assert.Contains("127.0.0.1:${SMTP_HOST_PORT:-1025}:1025", composeText);
        Assert.Contains("127.0.0.1:${SMTP_UI_HOST_PORT:-8025}:8025", composeText);
        Assert.Contains("MP_SMTP_AUTH: \"${SMTP_USER:?}:${SMTP_PASSWORD:?}\"", composeText);
        Assert.Contains("Smtp__Host: mailpit", composeText);
        Assert.Contains("Smtp__Username: ${SMTP_USER:?}", composeText);
        Assert.Contains("Smtp__Password: ${SMTP_PASSWORD:?}", composeText);
        Assert.Contains("Smtp__FromAddress: ${SMTP_FROM_ADDRESS:?}", composeText);

        Assert.Contains("SMTP_USER=", envExampleText);
        Assert.Contains("SMTP_PASSWORD=", envExampleText);
        Assert.Contains("SMTP_FROM_ADDRESS=noreply@helping-hands.example.test", envExampleText);
        Assert.Contains("Smtp__Host=localhost", envExampleText);
        Assert.Contains("Smtp__Port=1025", envExampleText);
    }

    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "compose.yaml")))
            dir = dir.Parent;

        return dir?.FullName ?? throw new InvalidOperationException("Could not locate repository root containing compose.yaml.");
    }
}