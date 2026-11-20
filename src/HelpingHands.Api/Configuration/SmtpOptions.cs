using System.ComponentModel.DataAnnotations;

namespace HelpingHands.Api.Configuration;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    [Required]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 1025;

    public string? Username { get; set; }

    public string? Password { get; set; }

    [Required]
    public string FromAddress { get; set; } = string.Empty;

    public bool UseSsl { get; set; }
}