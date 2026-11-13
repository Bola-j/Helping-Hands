using System.Globalization;
using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class RequestDecisionValidationTests
{
    [Theory]
    [InlineData("Rejected", null, "Goods", "1", "box")]
    [InlineData("Rejected", "   ", "Goods", "1", "box")]
    [InlineData("Unknown", "reason", "Goods", "1", "box")]
    [InlineData("Accepted", null, "Unsupported", "1", "box")]
    [InlineData("Accepted", null, "Goods", "0", "box")]
    [InlineData("Accepted", null, "Goods", "-1", "box")]
    [InlineData("Accepted", null, "Goods", "1.0001", "box")]
    [InlineData("Accepted", null, "Goods", "1000000000000000", "box")]
    [InlineData("Accepted", null, "Goods", "1", " ")]
    [InlineData("Accepted", null, "OrganizationalReferral", "2", "scope")]
    public async Task InvalidReviewInput_IsRejectedBeforeDatabaseAccess(
        string decision, string? reason, string category, string quantity, string unit)
    {
        using var client = Client();
        using var result = await DecideAsync(client, new
        {
            Decision = decision, Reason = reason, Category = category, ServiceSubtype = (string?)null,
            RequiredQuantity = decimal.Parse(quantity, CultureInfo.InvariantCulture), Unit = unit, Version = Guid.NewGuid()
        });
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("application/problem+json", result.Content.Headers.ContentType?.MediaType);
    }
}
