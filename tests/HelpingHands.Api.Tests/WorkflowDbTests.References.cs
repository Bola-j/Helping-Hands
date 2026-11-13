using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task ConcurrentBeneficiaries_ReceiveDistinctServerReferences()
    {
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        using var token = await beneficiary.GetAsync("/api/v1/auth/antiforgery");
        token.EnsureSuccessStatusCode();
        var requests = Enumerable.Range(0, 12).Select(_ => WorkflowDbFixture.BeneficiaryAsync(beneficiary));
        var records = await Task.WhenAll(requests);
        var references = records.Select(item => item.GetProperty("reference").GetString()).ToArray();
        Assert.All(references, reference => Assert.Matches("^BEN-[0-9]{8,}$", reference!));
        Assert.Equal(references.Length, references.Distinct().Count());
    }
}
