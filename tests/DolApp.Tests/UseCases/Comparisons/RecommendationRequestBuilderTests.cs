using DolApp.Application.Models;
using DolApp.Application.UseCases.Comparisons.Common;
using DolApp.Core.Entities;

namespace DolApp.Tests.UseCases.Comparisons;

/// Python /recommend isteğinin veritabanı satırlarından kurulması (A3 sözleşmesi §2, §6).
/// Liste id'leri seed'den bağımsız, testin kendi küçük tablolarıdır.

public class RecommendationRequestBuilderTests
{
    private static readonly Guid UserId = Guid.Parse("178eaf32-7cf1-4399-8957-6532e4c7fc91");
    private static readonly DateTime CompletedAtUtc = new(2026, 11, 27, 18, 0, 0, DateTimeKind.Utc);

    // Durum id'leri: 1 taslak, 2 onaylı, 3 silindi. Sonuç id'leri: 4 KT, 5 KD.
    private const long Approved = 2;
    private const long Deleted = 3;
    private const long RuleClean = 4;
    private const long RuleWithChanges = 5;

    private static readonly RecommendationLookups Lookups = RecommendationLookups.Create(
        subtypes: new[]
        {
            CreateSubtype(1, "tisort", categoryId: 1, defaultStyleIds: new List<long> { 1, 3 }, minTemp: 5, maxTemp: 40),
            CreateSubtype(9, "kot", categoryId: 2, defaultStyleIds: new List<long> { 1 }, minTemp: -20, maxTemp: 30),
            CreateSubtype(15, "elbise", categoryId: 3, defaultStyleIds: new List<long> { 1, 2, 4 }, minTemp: 5, maxTemp: 38),
            CreateSubtype(24, "spor_ayakkabi", categoryId: 5, defaultStyleIds: new List<long> { 1, 3 }, minTemp: -10, maxTemp: 40,
                preferredWhenHot: true),
        },
        categories: new[] { CreateCategory(1, "ust"), CreateCategory(2, "alt"), CreateCategory(3, "tek_parca"), CreateCategory(5, "ayakkabi") },
        colors: new[] { new Color { Id = 1, Value = "siyah" }, new Color { Id = 2, Value = "beyaz" } },
        tones: new[] { new Tone { Id = 1, Value = "acik" }, new Tone { Id = 3, Value = "koyu" } },
        styles: new[]
        {
            new Style { Id = 1, Value = "gunluk" }, new Style { Id = 2, Value = "is" },
            new Style { Id = 3, Value = "spor" }, new Style { Id = 4, Value = "davet" },
        },
        clothingStatuses: new[]
        {
            new ClothingStatus { Id = 1, Value = "taslak" }, new ClothingStatus { Id = Approved, Value = "onayli" },
            new ClothingStatus { Id = Deleted, Value = "silindi" },
        },
        weatherConditions: new[] { new WeatherCondition { Id = 3, Value = "kapali" } },
        resultStatuses: new[]
        {
            new ResultStatus { Id = RuleClean, Value = "KURAL_TEMIZ", ResultKey = "KT" },
            new ResultStatus { Id = RuleWithChanges, Value = "KURAL_DEGISIKLIKLE", ResultKey = "KD" },
        });

    [Fact]
    public void Build_MapsApprovedClothesToContractCodes()
    {
        var shirt = CreateClothe(18, subtypeId: 1, categoryId: 1, colorId: 2, toneId: 1, styleIds: new List<long> { 3, 1 });
        shirt.MinTemp = 0;
        shirt.MaxTemp = 32;
        var shoes = CreateClothe(36, subtypeId: 24, categoryId: 5, colorId: 2, toneId: 1, styleIds: new List<long> { 1 });

        var request = Build(approved: new[] { shoes, shirt });

        Assert.Equal(UserId, request.UserId);
        Assert.Equal(new long[] { 18, 36 }, request.Wardrobe.Select(item => item.Id));  // id sırasıyla

        var item = request.Wardrobe[0];
        Assert.Equal("tisort", item.Subtype);
        Assert.Equal("ust", item.Category);
        Assert.Equal("beyaz", item.Color);
        Assert.Equal("acik", item.Tone);
        Assert.Equal(new[] { "gunluk", "spor" }, item.Styles);  // kullanıcının tarzları, id sırasıyla
        Assert.Equal(0, item.MinTemp);                           // parçanın kendi aralığı
        Assert.Equal(32, item.MaxTemp);
        Assert.Equal("onayli", item.Status);
        Assert.True(request.Wardrobe[1].PreferredWhenHot);      // işaret alt türden
        Assert.Empty(request.RejectedOutfits);
        Assert.Empty(request.History);
    }

    [Fact]
    public void ToItem_WhenRangeAndStylesAreMissing_UsesSubtypeDefaults()
    {
        var shirt = CreateClothe(18, subtypeId: 1, categoryId: null, colorId: 2, toneId: 1, styleIds: null);

        var item = RecommendationRequestBuilder.ToItem(shirt, Lookups);

        Assert.NotNull(item);
        Assert.Equal("ust", item.Category);   // kategori alt türden
        Assert.Equal(5, item.MinTemp);
        Assert.Equal(40, item.MaxTemp);
        Assert.Equal(new[] { "gunluk", "spor" }, item.Styles);
    }

    [Fact]
    public void Build_SkipsClothesWithMissingLabels()
    {
        var noSubtype = CreateClothe(18, subtypeId: null, categoryId: 1, colorId: 2, toneId: 1, styleIds: new List<long> { 1 });
        var unknownColor = CreateClothe(19, subtypeId: 1, categoryId: 1, colorId: 99, toneId: 1, styleIds: new List<long> { 1 });
        var valid = CreateClothe(20, subtypeId: 9, categoryId: 2, colorId: 1, toneId: 3, styleIds: new List<long> { 1 });

        var request = Build(approved: new[] { noSubtype, unknownColor, valid });

        Assert.Equal(new long[] { 20 }, request.Wardrobe.Select(item => item.Id));
    }

    [Fact]
    public void Build_HistoryEntry_UsesResultKeyDecisionContextTemplateAndUtcOffset()
    {
        var comparison = CreateComparison(4826, RuleClean, rule: new() { 18, 26, 36 }, learned: new() { 19, 26, 36 },
            final: new() { 18, 26, 36 });

        var request = Build(history: new[] { comparison }, historyClothes: HistoryClothes());

        var entry = Assert.Single(request.History);
        Assert.Equal(4826, entry.ComparisonId);
        Assert.Equal("KT", entry.ResultKey);
        Assert.Equal(TimeSpan.Zero, entry.CompletedAt.Offset);
        Assert.Equal(CompletedAtUtc, entry.CompletedAt.UtcDateTime);
        Assert.Equal(8.1m, entry.Context.FeelsLikeTemp);
        Assert.Equal("kapali", entry.Context.WeatherCondition);
        Assert.Equal("is", entry.Context.EventStyle);
        Assert.Equal("ust_alt", entry.RuleOutfit.Template);
        Assert.Equal(new long[] { 18, 26, 36 }, entry.RuleOutfit.Items.Select(item => item.Id));
        Assert.Null(entry.FinalOutfit);  // KT'de son kombin gönderilmez
    }

    [Fact]
    public void Build_HistoryEntry_IncludesFinalOutfitOnlyForWithChangesResult()
    {
        var comparison = CreateComparison(1, RuleWithChanges, rule: new() { 18, 26, 36 }, learned: new() { 19, 26, 36 },
            final: new() { 19, 26, 36 });

        var request = Build(history: new[] { comparison }, historyClothes: HistoryClothes());

        var entry = Assert.Single(request.History);
        Assert.Equal("KD", entry.ResultKey);
        Assert.NotNull(entry.FinalOutfit);
        Assert.Equal(new long[] { 19, 26, 36 }, entry.FinalOutfit.Items.Select(item => item.Id));
    }

    [Fact]
    public void Build_HistoryEntry_SendsDeletedClothesAsSilindi()
    {
        var comparison = CreateComparison(1, RuleClean, rule: new() { 18, 26, 36 }, learned: new() { 19, 26, 36 });

        var request = Build(history: new[] { comparison }, historyClothes: HistoryClothes());

        var learnedTop = request.History[0].LearnedOutfit.Items[0];
        Assert.Equal(19, learnedTop.Id);
        Assert.Equal("silindi", learnedTop.Status);  // 19 sonradan silinmiş
    }

    [Fact]
    public void Build_SkipsHistoryEntryWhenAnItemIsMissing()
    {
        var complete = CreateComparison(1, RuleClean, rule: new() { 18, 26, 36 }, learned: new() { 19, 26, 36 });
        var withUnknownItem = CreateComparison(2, RuleClean, rule: new() { 18, 26, 999 }, learned: new() { 19, 26, 36 });

        var request = Build(history: new[] { complete, withUnknownItem }, historyClothes: HistoryClothes());

        Assert.Equal(new long[] { 1 }, request.History.Select(entry => entry.ComparisonId));
    }

    [Fact]
    public void Build_UsesOnePieceTemplateWhenOutfitHasADress()
    {
        var comparison = CreateComparison(1, RuleClean, rule: new() { 50, 36 }, learned: new() { 18, 26, 36 });

        var request = Build(history: new[] { comparison }, historyClothes: HistoryClothes());

        Assert.Equal("tek_parca", request.History[0].RuleOutfit.Template);
        Assert.Equal("ust_alt", request.History[0].LearnedOutfit.Template);
    }

    [Fact]
    public void CollectHistoryItemIds_ReturnsDistinctIdsFromAllOutfits()
    {
        var first = CreateComparison(1, RuleWithChanges, rule: new() { 18, 26, 36 }, learned: new() { 19, 26, 36 },
            final: new() { 18, 26, 36, 41 });
        var second = CreateComparison(2, RuleClean, rule: new() { 50, 36 }, learned: new() { 18, 26, 36 });

        var itemIds = RecommendationRequestBuilder.CollectHistoryItemIds(new[] { first, second });

        Assert.Equal(new long[] { 18, 19, 26, 36, 41, 50 }, itemIds.OrderBy(id => id));
    }

    // ---- yardımcılar ----

    private static OutfitRecommendationRequest Build(
        IReadOnlyCollection<Clothe>? approved = null,
        IReadOnlyCollection<Comparison>? history = null,
        IReadOnlyCollection<Clothe>? historyClothes = null)
    {
        var context = new RecommendationContext
        {
            FeelsLikeTemp = 9.4m,
            WeatherCondition = "kapali",
            TimeOfDay = "aksam",
            IsDaytime = false,
            EventStyle = "gunluk",
            WantsAccessories = false,
        };
        return RecommendationRequestBuilder.Build(
            UserId, context, approved ?? Array.Empty<Clothe>(), history ?? Array.Empty<Comparison>(),
            historyClothes ?? Array.Empty<Clothe>(), Lookups);
    }

    /// Geçmişteki parçalar: 18 tişört, 19 tişört (silinmiş), 26 kot, 36 spor ayakkabı, 50 elbise.
    private static List<Clothe> HistoryClothes()
    {
        var deletedShirt = CreateClothe(19, subtypeId: 1, categoryId: 1, colorId: 1, toneId: 3, styleIds: new List<long> { 1 });
        deletedShirt.StatusId = Deleted;
        return new List<Clothe>
        {
            CreateClothe(18, subtypeId: 1, categoryId: 1, colorId: 2, toneId: 1, styleIds: new List<long> { 1 }),
            deletedShirt,
            CreateClothe(26, subtypeId: 9, categoryId: 2, colorId: 1, toneId: 3, styleIds: new List<long> { 1 }),
            CreateClothe(36, subtypeId: 24, categoryId: 5, colorId: 2, toneId: 1, styleIds: new List<long> { 1 }),
            CreateClothe(50, subtypeId: 15, categoryId: 3, colorId: 1, toneId: 3, styleIds: new List<long> { 4 }),
        };
    }

    private static Comparison CreateComparison(
        long id, long resultId, List<long> rule, List<long> learned, List<long>? final = null) =>
        new()
        {
            Id = id,
            ResultId = resultId,
            RuleOutfit = rule,
            LearnedOutfit = learned,
            FinalOutfit = final,
            CompletedAt = CompletedAtUtc,
            Session = new ComparisonSession { FeelsLikeTemp = 8.1m, WeatherConditionId = 3, EventStyleId = 2 },
        };

    private static Clothe CreateClothe(long id, long? subtypeId, long? categoryId, long colorId, long toneId, List<long>? styleIds) =>
        new()
        {
            Id = id,
            SubtypeId = subtypeId,
            CategoryId = categoryId,
            MainColorId = colorId,
            ToneId = toneId,
            StyleIds = styleIds,
            StatusId = Approved,
        };

    private static Subtype CreateSubtype(
        long id, string value, long categoryId, List<long> defaultStyleIds, int minTemp, int maxTemp,
        bool preferredWhenHot = false) =>
        new()
        {
            Id = id,
            Value = value,
            DisplayName = value,
            CategoryId = categoryId,
            DefaultStyleIds = defaultStyleIds,
            DefaultSeasonIds = new List<long>(),
            MinTemp = minTemp,
            MaxTemp = maxTemp,
            PreferredWhenHot = preferredWhenHot,
        };

    private static Category CreateCategory(long id, string value) => new() { Id = id, Value = value, DisplayName = value };
}