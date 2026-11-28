using DolApp.Application.Models;
using DolApp.Infrastructure.Clients;
using Microsoft.Extensions.Logging.Abstractions;

namespace DolApp.Tests.Clients;

/// Taklit önerinin davranışı. Rastgelelik olmadığı için beklenen kombinler sabittir.
/// Küçük dolap: üst 1 ve 2, alt 3, ayakkabı 4 ve 5, dış giyim 6 (hepsi günlük), aksesuar 7, iş tarzı üst 8.

public class FakeOutfitRecommenderTests
{
    private readonly FakeOutfitRecommender _fakeOutfitRecommender = new(NullLogger<FakeOutfitRecommender>.Instance);

    [Fact]
    public async Task RecommendAsync_WhenCold_BuildsTwoOutfitsWithOuterwearAndAccessory()
    {
        var result = await _fakeOutfitRecommender.RecommendAsync(
            CreateRequest(feelsLikeTemp: 9.4m, eventStyle: "gunluk", wantsAccessories: true), CancellationToken.None);

        Assert.True(result.HasCandidates);
        Assert.Equal(new List<long> { 1, 3, 4, 6 }, result.RuleItemIds);
        Assert.Equal(new List<long> { 1, 3, 5, 6 }, result.LearnedItemIds);
        Assert.False(result.IsSameOutfit);
        Assert.Equal(new List<long> { 7 }, result.AccessoryIds);
        Assert.Equal("soguk_baslangic", result.LearnedPickMode);
    }

    [Fact]
    public async Task RecommendAsync_WhenWarmAndNoAccessories_SkipsOuterwearAndAccessory()
    {
        var result = await _fakeOutfitRecommender.RecommendAsync(
            CreateRequest(feelsLikeTemp: 20m, eventStyle: "gunluk", wantsAccessories: false), CancellationToken.None);

        Assert.Equal(new List<long> { 1, 3, 4 }, result.RuleItemIds);
        Assert.Empty(result.AccessoryIds);
    }

    [Fact]
    public async Task RecommendAsync_PrefersItemsMatchingEventStyle()
    {
        var result = await _fakeOutfitRecommender.RecommendAsync(
            CreateRequest(feelsLikeTemp: 20m, eventStyle: "is", wantsAccessories: false), CancellationToken.None);

        // Yalnızca üst 8 iş tarzında; alt ve ayakkabıda iş tarzı yok, bu yüzden hepsi kullanılır.
        Assert.Equal(8, result.RuleItemIds[0]);
        Assert.Equal(8, result.LearnedItemIds[0]);
    }

    [Fact]
    public async Task RecommendAsync_SkipsRejectedOutfitsRegardlessOfOrder()
    {
        var request = CreateRequest(feelsLikeTemp: 9.4m, eventStyle: "gunluk", wantsAccessories: false);
        request.RejectedOutfits.Add(new List<long> { 6, 4, 3, 1 });  // ilk kombin, farklı sırayla

        var result = await _fakeOutfitRecommender.RecommendAsync(request, CancellationToken.None);

        Assert.Equal(new List<long> { 1, 3, 5, 6 }, result.RuleItemIds);
        Assert.Equal(new List<long> { 2, 3, 4, 6 }, result.LearnedItemIds);
    }

    [Fact]
    public async Task RecommendAsync_WhenOnlyOneOutfitLeft_ReturnsSameOutfit_AndThenNoCandidates()
    {
        var request = CreateRequest(feelsLikeTemp: 20m, eventStyle: "gunluk", wantsAccessories: false);
        request.Wardrobe.RemoveAll(item => item.Id is 2 or 5 or 8);  // tek üst, tek alt, tek ayakkabı

        var single = await _fakeOutfitRecommender.RecommendAsync(request, CancellationToken.None);

        Assert.True(single.HasCandidates);
        Assert.True(single.IsSameOutfit);
        Assert.Equal(single.RuleItemIds, single.LearnedItemIds);

        request.RejectedOutfits.Add(new List<long> { 1, 3, 4 });
        var none = await _fakeOutfitRecommender.RecommendAsync(request, CancellationToken.None);

        Assert.False(none.HasCandidates);
    }

    private static OutfitRecommendationRequest CreateRequest(decimal feelsLikeTemp, string eventStyle, bool wantsAccessories) =>
        new()
        {
            UserId = Guid.NewGuid(),
            Context = new RecommendationContext
            {
                FeelsLikeTemp = feelsLikeTemp,
                WeatherCondition = "kapali",
                TimeOfDay = "aksam",
                IsDaytime = false,
                EventStyle = eventStyle,
                WantsAccessories = wantsAccessories,
            },
            Wardrobe = new List<RecommendationItem>
            {
                Item(1, "ust", "gunluk"),
                Item(2, "ust", "gunluk"),
                Item(3, "alt", "gunluk"),
                Item(4, "ayakkabi", "gunluk"),
                Item(5, "ayakkabi", "gunluk"),
                Item(6, "dis", "gunluk"),
                Item(7, "aksesuar", "gunluk"),
                Item(8, "ust", "is"),
            },
        };

    private static RecommendationItem Item(long id, string category, string style) =>
        new()
        {
            Id = id,
            Category = category,
            Subtype = "test",
            Color = "siyah",
            Tone = "koyu",
            Styles = new List<string> { style },
            MinTemp = -20,
            MaxTemp = 40,
            Status = "onayli",
        };
}