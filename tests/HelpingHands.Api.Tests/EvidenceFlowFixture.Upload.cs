using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowFixture
{
    public static async Task<JsonElement> UploadAsync(HttpClient client, Guid outcome, HttpStatusCode expected)
    {
        using var content = new MultipartFormDataContent();
        var document = new ByteArrayContent("%PDF-1.4\nfictional evidence\n%%EOF"u8.ToArray());
        document.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(document, "file", "fictional.pdf");
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/outcomes/{outcome}/attachments") { Content = content };
        request.Headers.Add("X-CSRF-TOKEN", await TokenAsync(client));
        using var response = await client.SendAsync(request);
        Assert.Equal(expected, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}
