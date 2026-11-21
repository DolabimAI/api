using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DolApp.Application.Interfaces;
using DolApp.Core.Errors;
using Microsoft.Extensions.Logging;

namespace DolApp.Infrastructure.Clients;

/// Python servisinin /analyze ve /health uçlarıyla konuşur 
/// Hata eşleştirmesi sözleşmedeki tabloyla aynı:
///   Python 422 image_unreadable  → uygulamaya 400 image_unreadable
///   diğer her hata, bozuk cevap, bağlantı yok ya da süre aşımı → 502 analysis_failed (ayrıntı yalnızca logda)

public class PythonClothingAnalyzer : IClothingAnalyzer
{
    private static readonly TimeSpan HealthCheckTimeout = TimeSpan.FromSeconds(5);

    private readonly HttpClient _httpClient;
    private readonly PythonOptions _pythonOptions;
    private readonly ILogger<PythonClothingAnalyzer> _logger;

    public PythonClothingAnalyzer(HttpClient httpClient, PythonOptions pythonOptions, ILogger<PythonClothingAnalyzer> logger)
    {
        _httpClient = httpClient;
        _pythonOptions = pythonOptions;
        _logger = logger;
    }

    public async Task<ClothingAnalysis> AnalyzeAsync(
        Stream imageStream, string fileName, string contentType, Guid userId, CancellationToken cancellationToken)
    {
        // multipart/form-data: "image" (dosya) ve "userId" (metin). Alan adları sözleşmeyle aynı olmalı.
        using var formContent = new MultipartFormDataContent();
        var imageContent = new StreamContent(imageStream);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        formContent.Add(imageContent, "image", fileName);
        formContent.Add(new StringContent(userId.ToString()), "userId");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync("analyze", formContent, cancellationToken);
        }
        catch (Exception exception) when (
            (exception is HttpRequestException or TaskCanceledException)
            && !cancellationToken.IsCancellationRequested)
        {
            // Bağlantı kurulamadı (servis kapalı) ya da süre aşıldı. Kullanıcı isteği iptal ettiyse buraya girilmez.
            throw CreateAnalysisFailedException($"Python /analyze isteği başarısız: {_pythonOptions.BaseUrl}", exception);
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
                return await ReadAnalysisAsync(response, cancellationToken);

            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.StatusCode == HttpStatusCode.UnprocessableEntity && ReadErrorCode(rawBody) == ErrorCodes.ImageUnreadable)
                throw new AppException(400, ErrorCodes.ImageUnreadable, ErrorMessages.ImageUnreadable);

            throw CreateAnalysisFailedException($"Python /analyze hata döndü: {(int)response.StatusCode} {rawBody}");
        }
    }

    public async Task CheckHealthAsync(CancellationToken cancellationToken)
    {
        // Açılışı bekletmemek için kısa süre: 5 sn içinde cevap yoksa "ulaşılamadı" sayılır.
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(HealthCheckTimeout);

        try
        {
            var healthResponse = await _httpClient.GetFromJsonAsync<PythonHealthResponse>("health", timeoutSource.Token);
            if (healthResponse?.Status == "ok")
                _logger.LogInformation("Python servisi ayakta: {BaseUrl}/health → ok", _pythonOptions.BaseUrl);
            else
                _logger.LogWarning("Python /health beklenen cevabı vermedi ({BaseUrl}). Fotoğraf analizi 502 dönebilir.",
                    _pythonOptions.BaseUrl);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            // Sözleşme: yalnızca status: "ok" beklenir. Ulaşılamazsa açılış durmaz, yalnızca uyarı yazılır.
            _logger.LogWarning("Python servisine ulaşılamadı: {BaseUrl}/health ({Reason}). Fotoğraf analizi 502 dönecek.",
                _pythonOptions.BaseUrl, exception.GetType().Name);
        }
    }

    /// 200 cevabını okur. Alanlardan biri eksikse sözleşmeye uymayan cevap sayılır (502).
    private async Task<ClothingAnalysis> ReadAnalysisAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        PythonAnalyzeResponse? analyzeResponse;
        try
        {
            analyzeResponse = await response.Content.ReadFromJsonAsync<PythonAnalyzeResponse>(cancellationToken);
        }
        catch (JsonException exception)
        {
            throw CreateAnalysisFailedException("Python /analyze cevabı JSON değil.", exception);
        }

        if (analyzeResponse is null
            || string.IsNullOrWhiteSpace(analyzeResponse.ImagePath)
            || string.IsNullOrWhiteSpace(analyzeResponse.Subtype)
            || analyzeResponse.SubtypeConfidence is null
            || string.IsNullOrWhiteSpace(analyzeResponse.Color)
            || string.IsNullOrWhiteSpace(analyzeResponse.Tone)
            || analyzeResponse.IsPatterned is null
            || string.IsNullOrWhiteSpace(analyzeResponse.RawColor))
        {
            // Görsel yazılmış olabilir: yolu loga yazılır, ama hangi alanın güvenilir olduğu bilinmediği için silinmez.
            throw CreateAnalysisFailedException(
                $"Python /analyze cevabında eksik alan var (imagePath: {analyzeResponse?.ImagePath ?? "yok"}).");
        }

        return new ClothingAnalysis
        {
            ImagePath = analyzeResponse.ImagePath,
            SubtypeValue = analyzeResponse.Subtype,
            SubtypeConfidence = analyzeResponse.SubtypeConfidence.Value,
            ColorValue = analyzeResponse.Color,
            ToneValue = analyzeResponse.Tone,
            IsPatterned = analyzeResponse.IsPatterned.Value,
            RawColor = analyzeResponse.RawColor,
        };
    }

    /// Hata gövdesindeki "code" alanı ({ "code": "image_unreadable", "message": "..." }). Okunamazsa null.
    private static string? ReadErrorCode(string rawBody)
    {
        try
        {
            using var jsonDocument = JsonDocument.Parse(rawBody);
            return jsonDocument.RootElement.TryGetProperty("code", out var codeElement) ? codeElement.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private AppException CreateAnalysisFailedException(string logDetail, Exception? innerException = null)
    {
        _logger.LogWarning(innerException, "Fotoğraf analizi başarısız. {Detail}", logDetail);
        return new AppException(502, ErrorCodes.AnalysisFailed, ErrorMessages.AnalysisFailed);
    }

    // ---- Python'un JSON cevapları (camelCase). Alanlar boş olabilir diye nullable; kontrol ReadAnalysisAsync'te. ----

    private sealed record PythonAnalyzeResponse(
        [property: JsonPropertyName("imagePath")] string? ImagePath,
        [property: JsonPropertyName("subtype")] string? Subtype,
        [property: JsonPropertyName("subtypeConfidence")] decimal? SubtypeConfidence,
        [property: JsonPropertyName("color")] string? Color,
        [property: JsonPropertyName("tone")] string? Tone,
        [property: JsonPropertyName("isPatterned")] bool? IsPatterned,
        [property: JsonPropertyName("rawColor")] string? RawColor);

    private sealed record PythonHealthResponse(
        [property: JsonPropertyName("status")] string? Status);
}