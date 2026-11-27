using DolApp.Application.Interfaces;
using DolApp.Application.Models;
using DolApp.Core.Constants;
using Microsoft.Extensions.Logging;

namespace DolApp.Infrastructure.Clients;

/// Python /recommend'in taklidi. YALNIZCA geliştirme içindir: appsettings.Development.json'da Python:UseFakeRecommender = true.
/// Amaç, Python servisi olmadan C5 ve C6'nın (ve ekranların) denenebilmesi. Öneri kalitesi hedeflenmez.
///
/// Davranış (rastgelelik yok; aynı istek her zaman aynı cevabı alır, testler buna dayanır):
///   - Yalnızca üst + alt + ayakkabı kombinleri kurulur (tek parça şablonu taklit edilmez).
///   - Her kategoride önce etkinlik tarzına uyan parçalar alınır; hiç yoksa o kategorinin bütün parçaları.
///   - Hissedilen sıcaklık 15 derecenin altındaysa ve dolapta dış giyim varsa ilk dış giyim kombine eklenir.
///   - Bu oturumda reddedilen kombinler (RejectedOutfits) atlanır; karşılaştırma sırasız (parça kümesi) yapılır.
///   - Kural kartı ilk uygun kombin, öğrenen kart ikinci uygun kombindir. Tek kombin kaldıysa iki kart aynıdır (IsSameOutfit).
///   - Hiç kombin kalmadıysa HasCandidates = false (Python'un no_candidates cevabı).
///   - Aksesuar istendiyse dolaptaki ilk aksesuar eklenir.
///   - LearnedPickMode her zaman "soguk_baslangic" (Python'da geçmişi olmayan kullanıcının modu).

public class FakeOutfitRecommender : IOutfitRecommender
{
    /// Bu sıcaklığın altında dış giyim eklenir. Python'un kuralı değil; taklidin basit eşiği.
    private const decimal OuterwearBelowFeelsLikeTemp = 15m;

    private readonly ILogger<FakeOutfitRecommender> _logger;

    public FakeOutfitRecommender(ILogger<FakeOutfitRecommender> logger)
    {
        _logger = logger;
    }

    public Task<OutfitRecommendationResult> RecommendAsync(
        OutfitRecommendationRequest request, CancellationToken cancellationToken)
    {
        var eventStyle = request.Context.EventStyle;
        var tops = PreferStyle(ItemsOf(request.Wardrobe, CategoryValues.Top), eventStyle);
        var bottoms = PreferStyle(ItemsOf(request.Wardrobe, CategoryValues.Bottom), eventStyle);
        var shoes = PreferStyle(ItemsOf(request.Wardrobe, CategoryValues.Shoes), eventStyle);
        var outer = request.Context.FeelsLikeTemp < OuterwearBelowFeelsLikeTemp
            ? PreferStyle(ItemsOf(request.Wardrobe, CategoryValues.Outer), eventStyle).FirstOrDefault()
            : null;

        var rejectedOutfits = request.RejectedOutfits.Select(outfit => outfit.ToHashSet()).ToList();

        // Reddedilmemiş ilk iki kombin yeterli; Take(2) üretimi orada durdurur.
        var candidates = AllOutfits(tops, bottoms, shoes, outer)
            .Where(outfit => !rejectedOutfits.Any(rejected => rejected.SetEquals(outfit)))
            .Take(2)
            .ToList();

        if (candidates.Count == 0)
        {
            _logger.LogInformation("Öneri taklidi: uygun kombin kalmadı (no_candidates).");
            return Task.FromResult(new OutfitRecommendationResult { HasCandidates = false });
        }

        var ruleItemIds = candidates[0];
        var learnedItemIds = candidates.Count > 1 ? candidates[1] : candidates[0];
        var accessoryIds = request.Context.WantsAccessories
            ? ItemsOf(request.Wardrobe, CategoryValues.Accessory).Take(1).Select(item => item.Id).ToList()
            : new List<long>();

        _logger.LogWarning(
            "Python:UseFakeRecommender = true: kombin önerisi taklitten geliyor (kural {RuleItemIds}, öğrenen {LearnedItemIds}).",
            string.Join(",", ruleItemIds), string.Join(",", learnedItemIds));

        return Task.FromResult(new OutfitRecommendationResult
        {
            HasCandidates = true,
            RuleItemIds = ruleItemIds,
            LearnedItemIds = learnedItemIds,
            IsSameOutfit = candidates.Count == 1,
            LearnedPickMode = "soguk_baslangic",
            AccessoryIds = accessoryIds,
        });
    }

    /// Üst × alt × ayakkabı sırasıyla kombinler (id sırasına göre); varsa dış giyim her kombine eklenir.
    private static IEnumerable<List<long>> AllOutfits(
        List<RecommendationItem> tops, List<RecommendationItem> bottoms, List<RecommendationItem> shoes,
        RecommendationItem? outer)
    {
        foreach (var top in tops)
            foreach (var bottom in bottoms)
                foreach (var shoe in shoes)
                {
                    var outfit = new List<long> { top.Id, bottom.Id, shoe.Id };
                    if (outer is not null)
                        outfit.Add(outer.Id);
                    yield return outfit;
                }
    }

    private static List<RecommendationItem> ItemsOf(IEnumerable<RecommendationItem> wardrobe, string category) =>
        wardrobe.Where(item => item.Category == category).OrderBy(item => item.Id).ToList();

    /// Etkinlik tarzına uyan parçalar; hiç yoksa hepsi.
    private static List<RecommendationItem> PreferStyle(List<RecommendationItem> items, string eventStyle)
    {
        var matching = items.Where(item => item.Styles.Contains(eventStyle)).ToList();
        return matching.Count > 0 ? matching : items;
    }
}