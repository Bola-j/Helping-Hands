using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowValidationTests
{
    [Theory]
    [InlineData(false, true, "Fictional need")]
    [InlineData(true, false, "Fictional need")]
    [InlineData(true, true, " ")]
    public async Task IntakeWithoutRequiredConfirmationOrDescription_IsRejected(
        bool authorityConfirmed, bool sharingAgreed, string description)
    {
        using var client = Client();
        using var response = await PostWithToken(client, "/api/v1/requests", new
        {
            BeneficiaryId = Guid.NewGuid(), Description = description,
            AuthorityConfirmed = authorityConfirmed, SharingAgreed = sharingAgreed
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task IntakeDescriptionBeyondLimit_IsRejected()
    {
        using var client = Client();
        using var response = await PostWithToken(client, "/api/v1/requests", new
        {
            BeneficiaryId = Guid.NewGuid(), Description = new string('a', 4001),
            AuthorityConfirmed = true, SharingAgreed = true
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
