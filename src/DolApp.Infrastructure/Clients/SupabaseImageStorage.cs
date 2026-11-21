using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DolApp.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DolApp.Infrastructure.Clients;

/// Supabase Storage'ın REST uçlarıyla konuşur (secret key ile; bucket private olduğu için politika gerekmez).
///   Süreli bağlantı: POST /storage/v1/object/sign/{bucket}/{yol}   → { "signedURL": "/object/sign/...?token=..." }
///   Toplu bağlantı:  POST /storage/v1/object/sign/{bucket}         → [ { "path", "signedURL", "error" } ]
///   Silme:           DELETE /storage/v1/object/{bucket}  gövde { "prefixes": [yol] }

public class SupabaseImageStorage : IImageStorage
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseOptions _supabaseOptions;
    private readonly StorageOptions _storageOptions;
    private readonly ILogger<SupabaseImageStorage> _logger;

    public SupabaseImageStorage(
        HttpClient httpClient,
        SupabaseOptions supabaseOptions,
        StorageOptions storageOptions,
        ILogger<SupabaseImageStorage> logger)
    {
        _httpClient = httpClient;
        _supabaseOptions = supabaseOptions;
        _storageOptions = storageOptions;
        _logger = logger;
    }

    public async Task<string?> CreateSignedUrlAsync(string imagePath, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"object/sign/{_storageOptions.Bucket}/{EscapePath(imagePath)}")
        {
            Content = JsonContent.Create(new { expiresIn = _storageOptions.SignedUrlSeconds })
        };
        AddApiKey(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // Görsel depoda yoksa parça yine döner, yalnızca görseli boş gelir; sebep loga yazılır.
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Süreli bağlantı üretilemedi: {ImagePath} {StatusCode} {Body}",
                imagePath, (int)response.StatusCode, rawBody);
            return null;
        }

        var signResponse = await response.Content.ReadFromJsonAsync<SignResponse>(cancellationToken);
        return signResponse?.SignedUrl is null ? null : ToFullUrl(signResponse.SignedUrl);
    }

    public async Task<IReadOnlyDictionary<string, string>> CreateSignedUrlsAsync(
        IReadOnlyCollection<string> imagePaths, CancellationToken cancellationToken)
    {
        if (imagePaths.Count == 0)
            return new Dictionary<string, string>();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"object/sign/{_storageOptions.Bucket}")
        {
            Content = JsonContent.Create(new { expiresIn = _storageOptions.SignedUrlSeconds, paths = imagePaths })
        };
        AddApiKey(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Supabase toplu bağlantı hatası: {(int)response.StatusCode} {rawBody}");
        }

        var signItems = await response.Content.ReadFromJsonAsync<List<SignItem>>(cancellationToken) ?? new List<SignItem>();
        var signedUrls = new Dictionary<string, string>();
        foreach (var signItem in signItems)
        {
            if (signItem.Path is null || signItem.SignedUrl is null || signItem.Error is not null)
            {
                _logger.LogWarning("Süreli bağlantı üretilemedi: {ImagePath} {Error}", signItem.Path, signItem.Error);
                continue;
            }
            signedUrls[signItem.Path] = ToFullUrl(signItem.SignedUrl);
        }
        return signedUrls;
    }

    public async Task DeleteAsync(string imagePath, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"object/{_storageOptions.Bucket}")
        {
            Content = JsonContent.Create(new { prefixes = new[] { imagePath } })
        };
        AddApiKey(request);

        // Görsel zaten yoksa Supabase 200 ve boş liste döner; bu yüzden tekrar çağırmak güvenli.
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Supabase görsel silme hatası: {imagePath} {(int)response.StatusCode} {rawBody}");
        }
    }

    /// YALNIZCA taklit modu (FakeClothingAnalyzer) için: gerçek akışta görseli Python yazar.
    /// Bucket yalnızca webp ve png kabul ettiği için küçük bir PNG yüklenir.
    public async Task UploadPlaceholderAsync(string imagePath, byte[] pngBytes, CancellationToken cancellationToken)
    {
        var imageContent = new ByteArrayContent(pngBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"object/{_storageOptions.Bucket}/{EscapePath(imagePath)}")
        {
            Content = imageContent
        };
        AddApiKey(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Supabase taklit görsel yükleme hatası: {imagePath} {(int)response.StatusCode} {rawBody}");
        }
    }

    /// Supabase "/object/sign/..." biçiminde göreli yol döner; başına depo adresi eklenir.
    private string ToFullUrl(string relativeSignedUrl) => $"{_supabaseOptions.Url}/storage/v1{relativeSignedUrl}";

    /// Yolun her parçası ayrı kodlanır; "/" ayraç olarak kalır.
    private static string EscapePath(string imagePath) =>
        string.Join("/", imagePath.Split('/').Select(Uri.EscapeDataString));

    /// SupabaseAuthClient'taki gibi: anahtar iki başlıkta aynı değerle gider.
    private void AddApiKey(HttpRequestMessage request)
    {
        request.Headers.Add("apikey", _supabaseOptions.SecretKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _supabaseOptions.SecretKey);
    }

    // ---- Supabase'in JSON cevaplarından yalnızca ihtiyacımız olan alanlar ----

    private sealed record SignResponse(
        [property: JsonPropertyName("signedURL")] string? SignedUrl);

    private sealed record SignItem(
        [property: JsonPropertyName("path")] string? Path,
        [property: JsonPropertyName("signedURL")] string? SignedUrl,
        [property: JsonPropertyName("error")] string? Error);
}