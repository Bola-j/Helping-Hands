using HelpingHands.Api.Configuration;

namespace HelpingHands.Api.Email;

public static class SmtpEmailRegistration
{
    public static IServiceCollection AddSmtpEmailSender(this IServiceCollection services)
    {
        services.AddOptions<SmtpOptions>()
            .BindConfiguration(SmtpOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddSingleton<ISmtpEmailSender, SmtpEmailSender>();
        return services;
    }
}