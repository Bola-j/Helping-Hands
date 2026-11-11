using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
namespace HelpingHands.Api.Storage;

public partial interface IEvidenceStorage
{
    Task<string> StageAsync(Stream content, string mediaType, CancellationToken cancellationToken);

    Task<string> PromoteAsync(string stagedKey, CancellationToken cancellationToken);

    Task<GetObjectResponse> OpenReadAsync(string key, CancellationToken cancellationToken);
}
