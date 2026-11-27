using DolApp.Application.Models;
using DolApp.Core.Constants;
using DolApp.Core.Entities;

namespace DolApp.Application.UseCases.Comparisons.Common;

/// Python /recommend isteğini veritabanı satırlarından kurar (A3 sözleşmesi §2, §3, §6). Veritabanına gitmez;
/// bu yüzden birim testiyle denenir (C4'teki GetWardrobeStatusResponse.Calculate gibi). C5 kullanır; C6 (İkisi de olmaz)
/// aynı isteği bu oturumda reddedilen kombinlerle tekrar kurar.
///
/// Kurallar:
///   - Etiketler kıyafetin GÜNCEL değerleri (karar 3); ıslak/sıcak işaretleri alt türden.
///   - Bir parça eksik etiketliyse (alt tür, renk, ton) ya da kodu bulunamazsa gönderilmez: Python onu reddederdi.
///   - Geçmişte bir kombinin parçalarından biri bulunamazsa o karar gönderilmez (yarım kombin öğrenmeyi bozar).
///     Handler, gönderilen ve okunan karar sayısını karşılaştırıp farkı loga yazar.
///   - Son kombin (finalOutfit) yalnızca "değişiklikle" kararlarında ve kaydedilmişse gider; yoksa null.
///   - Şablon: kombinde tek parça varsa tek_parca, yoksa ust_alt.
///   - Tarih saat dilimiyle gider (Python saat dilimsiz tarihi reddeder).

public static class RecommendationRequestBuilder
{
    public static OutfitRecommendationRequest Build(
        Guid userId,
        RecommendationContext context,
        IReadOnlyCollection<Clothe> approvedClothes,
        IReadOnlyCollection<Comparison> learningHistory,
        IReadOnlyCollection<Clothe> historyClothes,
        RecommendationLookups lookups,
        List<List<long>>? rejectedOutfits = null)
    {
        var wardrobe = approvedClothes
            .OrderBy(clothe => clothe.Id)
            .Select(clothe => ToItem(clothe, lookups))
            .OfType<RecommendationItem>()
            .ToList();

        var historyItemsById = historyClothes
            .Select(clothe => ToItem(clothe, lookups))
            .OfType<RecommendationItem>()
            .ToDictionary(item => item.Id);

        var history = learningHistory
            .Select(comparison => ToHistoryEntry(comparison, historyItemsById, lookups))
            .OfType<RecommendationHistoryEntry>()
            .ToList();

        return new OutfitRecommendationRequest
        {
            UserId = userId,
            Context = context,
            Wardrobe = wardrobe,
            RejectedOutfits = rejectedOutfits ?? new List<List<long>>(),
            History = history,
        };
    }

    /// Geçmiş kararlarda geçen bütün parça id'leri (kural, öğrenen ve son kombin). Handler bunları tek sorguyla okur.
    public static List<long> CollectHistoryItemIds(IEnumerable<Comparison> learningHistory)
    {
        return learningHistory
            .SelectMany(comparison => comparison.RuleOutfit
                .Concat(comparison.LearnedOutfit)
                .Concat(comparison.FinalOutfit ?? new List<long>()))
            .Distinct()
            .ToList();
    }

    /// Bir parçanın Python'daki karşılığı (parça nesnesi, §2). Gerekli etiketlerden biri yoksa null.
    public static RecommendationItem? ToItem(Clothe clothe, RecommendationLookups lookups)
    {
        if (clothe.SubtypeId is null || clothe.MainColorId is null || clothe.ToneId is null)
            return null;
        if (!lookups.Subtypes.TryGetValue(clothe.SubtypeId.Value, out var subtype))
            return null;

        var categoryId = clothe.CategoryId ?? subtype.CategoryId;
        if (!lookups.Categories.TryGetValue(categoryId, out var category)
            || !lookups.Colors.TryGetValue(clothe.MainColorId.Value, out var color)
            || !lookups.Tones.TryGetValue(clothe.ToneId.Value, out var tone)
            || !lookups.ClothingStatuses.TryGetValue(clothe.StatusId, out var status))
            return null;

        // Onaylı parçada tarz her zaman doludur; boşsa (eski veri) alt türün varsayılanları kullanılır.
        var styleIds = clothe.StyleIds is { Count: > 0 } ? clothe.StyleIds : subtype.DefaultStyleIds;
        var styles = styleIds
            .OrderBy(styleId => styleId)
            .Select(styleId => lookups.Styles.TryGetValue(styleId, out var style) ? style : null)
            .OfType<string>()
            .ToList();

        return new RecommendationItem
        {
            Id = clothe.Id,
            Subtype = subtype.Value,
            Category = category,
            Color = color,
            Tone = tone,
            IsPatterned = clothe.IsPatterned,
            Styles = styles,
            MinTemp = clothe.MinTemp ?? subtype.MinTemp,
            MaxTemp = clothe.MaxTemp ?? subtype.MaxTemp,
            AvoidWhenWet = subtype.AvoidWhenWet,
            PreferredWhenWet = subtype.PreferredWhenWet,
            PreferredWhenHot = subtype.PreferredWhenHot,
            Status = status,
        };
    }

    private static RecommendationHistoryEntry? ToHistoryEntry(
        Comparison comparison, IReadOnlyDictionary<long, RecommendationItem> itemsById, RecommendationLookups lookups)
    {
        if (comparison.CompletedAt is null
            || !lookups.ResultStatuses.TryGetValue(comparison.ResultId, out var resultStatus)
            || !lookups.WeatherConditions.TryGetValue(comparison.Session.WeatherConditionId, out var weatherCondition)
            || !lookups.Styles.TryGetValue(comparison.Session.EventStyleId, out var eventStyle))
            return null;

        var ruleOutfit = ToOutfit(comparison.RuleOutfit, itemsById);
        var learnedOutfit = ToOutfit(comparison.LearnedOutfit, itemsById);
        if (ruleOutfit is null || learnedOutfit is null)
            return null;

        // Son kombin yalnızca "değişiklikle" kararlarında anlamlı (sözleşme: KD, OD).
        RecommendationOutfit? finalOutfit = null;
        var isWithChanges = resultStatus.Value is ResultStatusValues.RuleWithChanges or ResultStatusValues.LearnedWithChanges;
        if (isWithChanges && comparison.FinalOutfit is { Count: > 0 })
        {
            finalOutfit = ToOutfit(comparison.FinalOutfit, itemsById);
            if (finalOutfit is null)
                return null;
        }

        return new RecommendationHistoryEntry
        {
            ComparisonId = comparison.Id,
            ResultKey = resultStatus.ResultKey,
            // Npgsql timestamptz'i UTC döner; Kind yine de açıkça UTC yapılır, offset +00:00 olur.
            CompletedAt = new DateTimeOffset(DateTime.SpecifyKind(comparison.CompletedAt.Value, DateTimeKind.Utc)),
            Context = new RecommendationHistoryContext
            {
                FeelsLikeTemp = comparison.Session.FeelsLikeTemp,
                WeatherCondition = weatherCondition,
                EventStyle = eventStyle,
            },
            RuleOutfit = ruleOutfit,
            LearnedOutfit = learnedOutfit,
            FinalOutfit = finalOutfit,
        };
    }

    /// Kombin: parçalar verilen sırayla. Biri bulunamazsa null (yarım kombin gönderilmez).
    private static RecommendationOutfit? ToOutfit(IEnumerable<long> itemIds, IReadOnlyDictionary<long, RecommendationItem> itemsById)
    {
        var items = new List<RecommendationItem>();
        foreach (var itemId in itemIds)
        {
            if (!itemsById.TryGetValue(itemId, out var item))
                return null;
            items.Add(item);
        }

        if (items.Count == 0)
            return null;

        var template = items.Any(item => item.Category == CategoryValues.OnePiece)
            ? OutfitTemplateValues.OnePiece
            : OutfitTemplateValues.TopAndBottom;

        return new RecommendationOutfit { Template = template, Items = items };
    }
}