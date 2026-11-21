using System.Reflection;
using HelpingHands.Api.Storage;

namespace HelpingHands.Api.Tests;

public class EvidenceStorageRecordingProxy : DispatchProxy
{
    private int _openReadCalls;
    public IEvidenceStorage Inner { get; set; } = null!;
    public int OpenReadCalls => Volatile.Read(ref _openReadCalls);

    protected override object? Invoke(MethodInfo? method, object?[]? arguments)
    {
        var operation = method ?? throw new InvalidOperationException("Missing storage method.");
        if (operation.Name == nameof(IEvidenceStorage.OpenReadAsync))
            Interlocked.Increment(ref _openReadCalls);
        return operation.Invoke(Inner, arguments);
    }
}
