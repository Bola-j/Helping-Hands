namespace HelpingHands.Api.Auth;

public static partial class AuthSetup
{
    private static (string, string, string, string) ReadSeedSettings(IConfiguration config)
    {
        var adminEmail = config["DemoSeed:AdminEmail"];
        var adminPassword = config["DemoSeed:AdminPassword"];
        var coordinatorEmail = config["DemoSeed:CoordinatorEmail"];
        var coordinatorPassword = config["DemoSeed:CoordinatorPassword"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword) ||
            string.IsNullOrWhiteSpace(coordinatorEmail) || string.IsNullOrWhiteSpace(coordinatorPassword) ||
            string.Equals(adminEmail, coordinatorEmail, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Configure distinct demo emails and passwords in the local environment.");
        return (adminEmail, adminPassword, coordinatorEmail, coordinatorPassword);
    }
}
