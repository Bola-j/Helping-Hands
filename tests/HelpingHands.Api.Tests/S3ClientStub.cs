using System.Reflection;
using Amazon.S3;
using Amazon.S3.Model;

namespace HelpingHands.Api.Tests;

public class S3ClientStub : DispatchProxy
{
    public List<string> Calls { get; } = [];
    public bool FailPut { get; set; }
    public bool FailCopy { get; set; }
    public PutObjectRequest? Upload { get; private set; }

    protected override object? Invoke(MethodInfo? method, object?[]? arguments)
    {
        var name = method?.Name ?? throw new InvalidOperationException("Missing method name.");
        Calls.Add(name);
        return name switch
        {
            nameof(IAmazonS3.PutObjectAsync) => UploadResult((PutObjectRequest)arguments![0]!),
            nameof(IAmazonS3.CopyObjectAsync) => FailCopy ? Task.FromException<CopyObjectResponse>(new IOException("copy failed")) : Task.FromResult(new CopyObjectResponse()),
            nameof(IAmazonS3.DeleteObjectAsync) => Task.FromResult(new DeleteObjectResponse()),
            _ => throw new NotSupportedException(name)
        };
    }

    private Task<PutObjectResponse> UploadResult(PutObjectRequest request)
    {
        Upload = request;
        return FailPut ? Task.FromException<PutObjectResponse>(new IOException("upload failed")) : Task.FromResult(new PutObjectResponse());
    }
}
