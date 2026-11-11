using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
namespace HelpingHands.Api.Storage;

public partial interface IEvidenceStorage
{
    Task DeleteAsync(string key, CancellationToken cancellationToken);

    Task<int> CleanupStagedBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken);
}
