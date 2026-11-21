using System.Net;
using System.Net.Http.Headers;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowFixture
{
    public static async Task<HttpStatusCode> UploadRejectedAsync(HttpClient client, Guid outcome, string type, string name, byte[] bytes)
    {
        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(type);
        content.Add(file, "file", name);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/outcomes/{outcome}/attachments") { Content = content };
        request.Headers.Add("X-CSRF-TOKEN", await TokenAsync(client));
        using var response = await client.SendAsync(request);
        return response.StatusCode;
    }
}
