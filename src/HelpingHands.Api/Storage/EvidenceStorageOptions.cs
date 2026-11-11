using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
namespace HelpingHands.Api.Storage;

public sealed class EvidenceStorageOptions
{
    public const string SectionName = "EvidenceStorage";
    public string ServiceUrl { get; set; } = "";
    public string Bucket { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
}
