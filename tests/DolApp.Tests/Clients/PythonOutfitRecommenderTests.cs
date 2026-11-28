using System.Net;
using System.Text.Json;
using DolApp.Application.Models;
using DolApp.Core.Errors;
using DolApp.Infrastructure.Clients;
using DolApp.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace DolApp.Tests.Clients;

/// Python /recommend istemcisinin istek biçimi ve sözleşmedeki hata eşleştirmesi (contracts/python-api.md, 6. bölüm).
/// Gerçek Python'a gidilmez: cevaplar StubHttpMessageHandler'dan gelir.

public class PythonOutfitRecommenderTests
{
    private static readonly Guid TestUserId = Guid.Parse("178eaf32-7cf1-4399-8957-6532e4c7fc91");

    /// contracts/examples/recommend-response.json ile aynı.
    private const string OkResponse = """
        {
          "status": "ok",
          "ruleOutfit": { "template": "ust_alt", "itemIds": [1, 10, 17, 24] },
          "learnedOutfit": { "template": "ust_alt", "itemIds": [5, 9, 17, 24] },
          "isSameOutfit": false,
          "learnedPickMode": "model",
          "accessoryIds": [34]
        }
        """;

    private readonly StubHttpMessageHandler _stubHttpMessageHandler = new();

    [Fact]
    public async Task RecommendAsync_WhenPythonReturnsOk_MapsAllFields()
    {
        _stubHttpMessageHandler.ResponseBody = OkResponse;

        var result = await RecommendAsync();

        Assert.True(result.HasCandidates);
        Assert.Equal(new List<long> { 1, 10, 17, 24 }, result.RuleItemIds);
        Assert.Equal(new List<long> { 5, 9, 17, 24 }, result.LearnedItemIds);
        Assert.False(result.IsSameOutfit);
        Assert.Equal("model", result.LearnedPickMode);
        Assert.Equal(new List<long> { 34 }, result.AccessoryIds);
    }

    [Fact]
    public async Task RecommendAsync_WhenSameOutfit_MapsIsSameOutfit()
    {
        _stubHttpMessageHandler.ResponseBody = """
            {
              "status": "ok",
              "ruleOutfit": { "template": "ust_alt", "itemIds": [1, 9, 24] },
              "learnedOutfit": { "template": "ust_alt", "itemIds": [1, 9, 24] },
              "isSameOutfit": true,
              "learnedPickMode": "soguk_baslangic",
              "accessoryIds": []
            }
            """;

        var result = await RecommendAsync();

        Assert.True(result.HasCandidates);
        Assert.True(result.IsSameOutfit);
        Assert.Empty(result.AccessoryIds);
    }

    [Fact]
    public async Task RecommendAsync_WhenNoCandidates_ReturnsHasCandidatesFalse()
    {
        // Aday yok bir hata değil: istemci fırlatmaz, ne yapılacağına handler karar verir (C5: 422, C6: noMoreCandidates).
        _stubHttpMessageHandler.ResponseBody = """{ "status": "no_candidates" }""";

        var result = await RecommendAsync();

        Assert.False(result.HasCandidates);
        Assert.Empty(result.RuleItemIds);
        Assert.Empty(result.LearnedItemIds);
    }

    [Fact]
    public async Task RecommendAsync_SendsContractJsonToRecommendEndpoint()
    {
        // Python bilinmeyen alanı ve saat dilimsiz tarihi reddeder (422): alan adları ve tarih biçimi sözleşmeyle aynı olmalı.
        _stubHttpMessageHandler.ResponseBody = OkResponse;

        await RecommendAsync();

        Assert.Equal("/recommend", _stubHttpMessageHandler.LastRequest!.RequestUri!.AbsolutePath);
        using var json = JsonDocument.Parse(_stubHttpMessageHandler.LastRequestBody!);
        var root = json.RootElement;

        Assert.Equal(new[] { "userId", "context", "wardrobe", "rejectedOutfits", "history" }, PropertyNames(root));
        Assert.Equal(TestUserId.ToString(), root.GetProperty("userId").GetString());
        Assert.Equal(
            new[] { "feelsLikeTemp", "weatherCondition", "timeOfDay", "isDaytime", "eventStyle", "wantsAccessories" },
            PropertyNames(root.GetProperty("context")));
        Assert.Equal(9.4m, root.GetProperty("context").GetProperty("feelsLikeTemp").GetDecimal());
        Assert.Equal(
            new[] { "id", "subtype", "category", "color", "tone", "isPatterned", "styles", "minTemp", "maxTemp",
                    "avoidWhenWet", "preferredWhenWet", "preferredWhenHot", "status" },
            PropertyNames(root.GetProperty("wardrobe")[0]));
        Assert.Equal(18, root.GetProperty("rejectedOutfits")[0][0].GetInt64());

        var history = root.GetProperty("history")[0];
        Assert.Equal(
            new[] { "comparisonId", "resultKey", "completedAt", "context", "ruleOutfit", "learnedOutfit", "finalOutfit" },
            PropertyNames(history));
        Assert.Equal(JsonValueKind.Null, history.GetProperty("finalOutfit").ValueKind);
        // Saat dilimi yazılmalı (Python saat dilimsiz tarihi reddeder).
        Assert.EndsWith("+00:00", history.GetProperty("completedAt").GetString());
    }

    [Fact]
    public async Task RecommendAsync_WhenPythonReturns422InvalidRequest_Throws502()
    {
        // 422, C#'ın isteğinin sözleşmeye uymadığı anlamına gelir: bizim hatamız, kullanıcıya 502.
        _stubHttpMessageHandler.ResponseStatusCode = HttpStatusCode.UnprocessableEntity;
        _stubHttpMessageHandler.ResponseBody = """{ "code": "invalid_request", "message": "Bilinmeyen hava durumu: x" }""";

        await AssertRecommendationFailedAsync();
    }

    [Fact]
    public async Task RecommendAsync_WhenPythonReturns500_Throws502()
    {
        _stubHttpMessageHandler.ResponseStatusCode = HttpStatusCode.InternalServerError;
        _stubHttpMessageHandler.ResponseBody = """{ "code": "internal_error", "message": "..." }""";

        await AssertRecommendationFailedAsync();
    }

    [Fact]
    public async Task RecommendAsync_WhenPythonIsDown_Throws502()
    {
        _stubHttpMessageHandler.ExceptionToThrow = new HttpRequestException("Connection refused");

        await AssertRecommendationFailedAsync();
    }

    [Fact]
    public async Task RecommendAsync_WhenTimeoutExpires_Throws502()
    {
        // HttpClient 30 sn aşımını TaskCanceledException olarak fırlatır.
        _stubHttpMessageHandler.ExceptionToThrow = new TaskCanceledException("Timeout");

        await AssertRecommendationFailedAsync();
    }

    [Fact]
    public async Task RecommendAsync_WhenResponseMissesAField_Throws502()
    {
        _stubHttpMessageHandler.ResponseBody = """
            { "status": "ok", "ruleOutfit": { "template": "ust_alt", "itemIds": [1, 10, 17] } }
            """;

        await AssertRecommendationFailedAsync();
    }

    [Fact]
    public async Task RecommendAsync_WhenResponseIsNotJson_Throws502()
    {
        _stubHttpMessageHandler.ResponseBody = "Internal Server Error";

        await AssertRecommendationFailedAsync();
    }

    private async Task AssertRecommendationFailedAsync()
    {
        var appException = await Assert.ThrowsAsync<AppException>(RecommendAsync);

        Assert.Equal(502, appException.StatusCode);
        Assert.Equal(ErrorCodes.RecommendationFailed, appException.Code);
    }

    private async Task<OutfitRecommendationResult> RecommendAsync()
    {
        var httpClient = new HttpClient(_stubHttpMessageHandler) { BaseAddress = new Uri("http://python.test/") };
        var pythonOutfitRecommender = new PythonOutfitRecommender(
            httpClient,
            new PythonOptions { BaseUrl = "http://python.test" },
            NullLogger<PythonOutfitRecommender>.Instance);

        return await pythonOutfitRecommender.RecommendAsync(CreateRequest(), CancellationToken.None);
    }

    /// Bütün alanları dolu küçük bir istek: bir parça, bir red, bir geçmiş karar (finalOutfit boş).
    private static OutfitRecommendationRequest CreateRequest()
    {
        var item = new RecommendationItem
        {
            Id = 18,
            Subtype = "tisort",
            Category = "ust",
            Color = "beyaz",
            Tone = "acik",
            IsPatterned = false,
            Styles = new List<string> { "gunluk", "spor" },
            MinTemp = 5,
            MaxTemp = 40,
            Status = "onayli",
        };
        var outfit = new RecommendationOutfit { Template = "ust_alt", Items = new List<RecommendationItem> { item } };

        return new OutfitRecommendationRequest
        {
            UserId = TestUserId,
            Context = new RecommendationContext
            {
                FeelsLikeTemp = 9.4m,
                WeatherCondition = "parcali_bulutlu",
                TimeOfDay = "aksam",
                IsDaytime = false,
                EventStyle = "gunluk",
                WantsAccessories = true,
            },
            Wardrobe = new List<RecommendationItem> { item },
            RejectedOutfits = new List<List<long>> { new List<long> { 18, 27, 38, 33 } },
            History = new List<RecommendationHistoryEntry>
            {
                new RecommendationHistoryEntry
                {
                    ComparisonId = 1, ResultKey = "KT",
                    CompletedAt = new DateTimeOffset(2026, 11, 27, 18, 0, 0, TimeSpan.Zero),
                    Context = new RecommendationHistoryContext
                    {
                        FeelsLikeTemp = 10.0m, WeatherCondition = "kapali", EventStyle = "gunluk",
                    },
                    RuleOutfit = outfit, LearnedOutfit = outfit, FinalOutfit = null,
                },
            },
        };
    }

    private static string[] PropertyNames(JsonElement element) =>
        element.EnumerateObject().Select(property => property.Name).ToArray();
}