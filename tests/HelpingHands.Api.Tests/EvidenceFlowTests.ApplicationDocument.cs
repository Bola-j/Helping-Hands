using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    [Fact]
    public async Task ApplicationDocument_IsDiscoverablePrivatelyAndEnablesApproval()
    {
        using var applicant = fixture.Client();
        using var coordinator = fixture.Client();
        using var outsider = fixture.Client();
        await fixture.LoginAsync(coordinator, fixture.Coordinator);
        await fixture.RegisterAndLoginAsync(outsider, $"outsider-{Guid.NewGuid():N}@example.test");
        var email = $"document-{Guid.NewGuid():N}@example.test";
        var signup = await EvidenceFlowFixture.PostJsonAsync(applicant, "/api/v1/auth/register/charity", new
        { RepresentativeDisplayName = "Fictional", Email = email, fixture.Password, OrganizationName = "Fictional", ContactDetails = "Fictional", RegistrationInformation = (string?)null }, HttpStatusCode.Created);
        await fixture.LoginAsync(applicant, email);
        var id = signup.GetProperty("recipientId").GetGuid();
        var attachment = await EvidenceFlowFixture.UploadApplicationAsync(applicant, id);
        await AssertApplicationDocumentAccessAsync(applicant, coordinator, outsider, id, attachment);
        using var detailResponse = await coordinator.GetAsync($"/api/v1/recipients/{id}");
        var detail = await EvidenceFlowFixture.JsonAsync(detailResponse, HttpStatusCode.OK);
        var approved = await EvidenceFlowFixture.PostJsonAsync(coordinator, $"/api/v1/recipients/{id}/decisions",
            new { Decision = "Approved", Reason = (string?)null, Version = Version(detail) }, HttpStatusCode.OK);
        Assert.True(approved.GetProperty("active").GetBoolean());
    }

    private async Task AssertApplicationDocumentAccessAsync(HttpClient applicant, HttpClient coordinator, HttpClient outsider, Guid id, Guid attachment)
    {
        foreach (var client in new[] { applicant, coordinator })
        {
            using var response = await client.GetAsync($"/api/v1/recipients/{id}");
            var detail = await EvidenceFlowFixture.JsonAsync(response, HttpStatusCode.OK);
            var item = Assert.Single(detail.GetProperty("attachments").EnumerateArray());
            Assert.Equal(attachment, item.GetProperty("id").GetGuid());
            Assert.Equal("fictional.pdf", item.GetProperty("originalName").GetString());
            Assert.False(item.TryGetProperty("storageKey", out _));
            using var content = await client.GetAsync($"/api/v1/attachments/{attachment}/content");
            Assert.Equal(HttpStatusCode.OK, content.StatusCode);
        }
        using var hidden = await outsider.GetAsync($"/api/v1/recipients/{id}");
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
        using var denied = await outsider.GetAsync($"/api/v1/attachments/{attachment}/content");
        Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode);
        using var anonymous = fixture.Client();
        using var unauthenticated = await anonymous.GetAsync($"/api/v1/attachments/{attachment}/content");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticated.StatusCode);
    }
}
