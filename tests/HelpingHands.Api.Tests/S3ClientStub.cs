using System.Reflection;
using Amazon.S3;
using Amazon.S3.Model;

namespace HelpingHands.Api.Tests;

public class S3ClientStub : DispatchProxy
{
    public List<string> Calls { get; } = [];
    public bool FailPut { get; set; }
    public bool FailCopy { get; set; }
    public bool FailDelete { get; set; }
    public bool CancelPutAfterWrite { get; set; }
    public PutObjectRequest? Upload { get; private set; }
    public List<S3Object> Objects { get; } = [];

    protected override object? Invoke(MethodInfo? method, object?[]? arguments)
    {
        var name = method?.Name ?? throw new InvalidOperationException("Missing method name.");
        Calls.Add(name);
        if (name == nameof(IAmazonS3.ListObjectsV2Async))
        {
            var request = (ListObjectsV2Request)arguments![0]!;
            return Task.FromResult(new ListObjectsV2Response
            {
                IsTruncated = false,
                S3Objects = Objects.Where(item => item.Key.StartsWith(request.Prefix ?? "", StringComparison.Ordinal)).ToList()
            });
        }
        if (name == nameof(IAmazonS3.DeleteObjectAsync))
        {
            if (FailDelete)
                return Task.FromException<DeleteObjectResponse>(new IOException("delete failed"));
            Objects.RemoveAll(item => item.Key == (string)arguments![1]!);
            return Task.FromResult(new DeleteObjectResponse());
        }
        return name switch
        {
            nameof(IAmazonS3.PutObjectAsync) => UploadResult((PutObjectRequest)arguments![0]!),
            nameof(IAmazonS3.CopyObjectAsync) => FailCopy ? Task.FromException<CopyObjectResponse>(new IOException("copy failed")) : Task.FromResult(new CopyObjectResponse()),
            _ => throw new NotSupportedException(name)
        };
    }

    private Task<PutObjectResponse> UploadResult(PutObjectRequest request)
    {
        Upload = request;
        if (CancelPutAfterWrite)
        {
            Objects.Add(new S3Object { Key = request.Key, LastModified = DateTime.UtcNow });
            return Task.FromException<PutObjectResponse>(new OperationCanceledException("Upload interrupted."));
        }
        if (!FailPut)
            Objects.Add(new S3Object { Key = request.Key, LastModified = DateTime.UtcNow });
        return FailPut ? Task.FromException<PutObjectResponse>(new IOException("upload failed")) : Task.FromResult(new PutObjectResponse());
    }
}
