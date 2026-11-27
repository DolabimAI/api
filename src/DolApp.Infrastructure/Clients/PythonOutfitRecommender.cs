using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DolApp.Application.Interfaces;
using DolApp.Application.Models;
using DolApp.Core.Errors;
using Microsoft.Extensions.Logging;

namespace DolApp.Infrastructure.Clients;

/// Python servisinin /recommend ucuyla konuşur (A3 sözleşmesi §6).
/// İstek JSON'u camelCase gider (JsonSerializerDefaults.Web); Python bilinmeyen alanı reddettiği için
/// istek sınıflarına (Application/Models/OutfitRecommendationModels.cs) sözleşmede olmayan özellik eklenmez.
/// Hata eşleştirmesi sözleşmedeki tabloyla aynı:
///   200 status "ok"             → iki kart
///   200 status "no_candidates"  → HasCandidates = false (hata değil; ne yapılacağına handler karar verir)
///   422 invalid_request         → 502 recommendation_failed (C# yanlış istek göndermiş demektir; ayrıntı logda)
///   diğer her hata, bozuk cevap, bağlantı yok ya da süre aşımı → 502 recommendation_failed

public class PythonOutfitRecommender : IOutfitRecommender
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly PythonOptions _pythonOptions;
    private readonly ILogger<PythonOutfitRecommender> _logger;

    public PythonOutfitRecommender(HttpClient httpClient, PythonOptions pythonOptions, ILogger<PythonOutfitRecommender> logger)
    {
        _httpClient = httpClient;
        _pythonOptions = pythonOptions;
        _logger = logger;
    }

    public async Task<OutfitRecommendationResult> RecommendAsync(
        OutfitRecommendationRequest request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("recommend", request, JsonOptions, cancellationToken);
        }
        catch (Exception exception) when (
            (exception is HttpRequestException or TaskCanceledException)
            && !cancellationToken.IsCancellationRequested)
        {
            // Bağlantı kurulamadı (servis kapalı) ya da süre aşıldı. Kullanıcı isteği iptal ettiyse buraya girilmez.
            throw CreateRecommendationFailedException($"Python /recommend isteği başarısız: {_pythonOptions.BaseUrl}", exception);
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
                return await ReadResultAsync(response, cancellationToken);

            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                // Python isteği sözleşmeye uygun bulmadı: alan adı, tür ya da kod hatası C# tarafındadır.
                throw CreateRecommendationFailedException($"Python /recommend isteği reddetti (C# isteği sözleşmeye uymuyor): {rawBody}");
            }

            throw CreateRecommendationFailedException($"Python /recommend hata döndü: {(int)response.StatusCode} {rawBody}");
        }
    }

    /// 200 cevabını okur. "ok" cevabında alanlardan biri eksikse sözleşmeye uymayan cevap sayılır (502).
    private async Task<OutfitRecommendationResult> ReadResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        PythonRecommendResponse? recommendResponse;
        try
        {
            recommendResponse = await response.Content.ReadFromJsonAsync<PythonRecommendResponse>(JsonOptions, cancellationToken);
        }
        catch (JsonException exception)
        {
            throw CreateRecommendationFailedException("Python /recommend cevabı JSON değil.", exception);
        }

        if (recommendResponse?.Status == "no_candidates")
            return new OutfitRecommendationResult { HasCandidates = false };

        var ruleItemIds = recommendResponse?.RuleOutfit?.ItemIds;
        var learnedItemIds = recommendResponse?.LearnedOutfit?.ItemIds;
        var isSameOutfit = recommendResponse?.IsSameOutfit;
        var learnedPickMode = recommendResponse?.LearnedPickMode;
        var accessoryIds = recommendResponse?.AccessoryIds;

        if (recommendResponse?.Status != "ok"
            || ruleItemIds is null || ruleItemIds.Count == 0
            || learnedItemIds is null || learnedItemIds.Count == 0
            || isSameOutfit is null
            || string.IsNullOrWhiteSpace(learnedPickMode)
            || accessoryIds is null)
        {
            throw CreateRecommendationFailedException(
                $"Python /recommend cevabı sözleşmeye uymuyor (status: {recommendResponse?.Status ?? "yok"}).");
        }

        return new OutfitRecommendationResult
        {
            HasCandidates = true,
            RuleItemIds = ruleItemIds,
            LearnedItemIds = learnedItemIds,
            IsSameOutfit = isSameOutfit.Value,
            LearnedPickMode = learnedPickMode,
            AccessoryIds = accessoryIds,
        };
    }

    private AppException CreateRecommendationFailedException(string logDetail, Exception? innerException = null)
    {
        _logger.LogWarning(innerException, "Kombin önerisi alınamadı. {Detail}", logDetail);
        return new AppException(502, ErrorCodes.RecommendationFailed, ErrorMessages.RecommendationFailed);
    }

    // ---- Python'un JSON cevabı (camelCase). Alanlar eksik olabilir diye nullable; kontrol ReadResultAsync'te. ----

    private sealed record PythonRecommendResponse(
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("ruleOutfit")] PythonOutfit? RuleOutfit,
        [property: JsonPropertyName("learnedOutfit")] PythonOutfit? LearnedOutfit,
        [property: JsonPropertyName("isSameOutfit")] bool? IsSameOutfit,
        [property: JsonPropertyName("learnedPickMode")] string? LearnedPickMode,
        [property: JsonPropertyName("accessoryIds")] List<long>? AccessoryIds);

    private sealed record PythonOutfit(
        [property: JsonPropertyName("template")] string? Template,
        [property: JsonPropertyName("itemIds")] List<long>? ItemIds);
}