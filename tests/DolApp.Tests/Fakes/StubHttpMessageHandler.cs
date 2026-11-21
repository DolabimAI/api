using System.Net;
using System.Text;

namespace DolApp.Tests.Fakes;

/// HttpClient'ın ağa çıkmadan cevap vermesi için. Test, gelecek cevabı (ya da fırlatılacak hatayı) önceden belirler.
/// Gönderilen son istek LastRequest'te durur; alan adlarını kontrol etmek için.

public class StubHttpMessageHandler : HttpMessageHandler
{
    public HttpStatusCode ResponseStatusCode { get; set; } = HttpStatusCode.OK;
    public string ResponseBody { get; set; } = "{}";

    /// Doluysa cevap yerine bu fırlatılır (bağlantı hatası, zaman aşımı).
    public Exception? ExceptionToThrow { get; set; }

    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

        if (ExceptionToThrow is not null)
            throw ExceptionToThrow;

        return new HttpResponseMessage(ResponseStatusCode)
        {
            Content = new StringContent(ResponseBody, Encoding.UTF8, "application/json"),
        };
    }
}