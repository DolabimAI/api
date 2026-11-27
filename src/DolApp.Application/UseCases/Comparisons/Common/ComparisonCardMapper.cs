using DolApp.Core.Entities;

namespace DolApp.Application.UseCases.Comparisons.Common;

/// comparisons satırını uygulamanın kart biçimine çevirir. Veritabanına gitmez (birim testiyle denenir); C6 da kullanır.
/// Hangi kombinin solda duracağı yalnızca left_method_id'den anlaşılır; cevaba yöntem bilgisi girmez.

public static class ComparisonCardMapper
{
    public static ComparisonCard ToCard(
        Comparison comparison,
        long ruleMethodId,
        IReadOnlyDictionary<long, Clothe> clothesById,
        IReadOnlyDictionary<string, string> imageUrls)
    {
        var isSingleCard = comparison.RuleOutfit.ToHashSet().SetEquals(comparison.LearnedOutfit);
        var ruleIsLeft = comparison.LeftMethodId == ruleMethodId;
        var leftItemIds = ruleIsLeft ? comparison.RuleOutfit : comparison.LearnedOutfit;
        var rightItemIds = ruleIsLeft ? comparison.LearnedOutfit : comparison.RuleOutfit;

        return new ComparisonCard
        {
            Id = comparison.Id,
            SequenceNo = comparison.SequenceNo,
            IsSingleCard = isSingleCard,
            Left = leftItemIds.Select(itemId => ToOutfitItem(itemId, clothesById, imageUrls)).ToList(),
            Right = isSingleCard
                ? new List<OutfitCardItem>()
                : rightItemIds.Select(itemId => ToOutfitItem(itemId, clothesById, imageUrls)).ToList(),
            Accessories = (comparison.Accessories ?? new List<long>())
                .Select(itemId => new AccessoryCardItem { Id = itemId, ImageUrl = ImageUrlOf(itemId, clothesById, imageUrls) })
                .ToList(),
        };
    }

    /// Kartlardaki parçaların depo yolları (tekrarsız, boş olanlar hariç). Handler bunlar için tek istekte süreli bağlantı alır.
    public static List<string> CollectImagePaths(Comparison comparison, IReadOnlyDictionary<long, Clothe> clothesById)
    {
        return comparison.RuleOutfit
            .Concat(comparison.LearnedOutfit)
            .Concat(comparison.Accessories ?? new List<long>())
            .Distinct()
            .Select(itemId => clothesById.TryGetValue(itemId, out var clothe) ? clothe.ImagePath : null)
            .Where(imagePath => !string.IsNullOrEmpty(imagePath))
            .Select(imagePath => imagePath!)
            .ToList();
    }

    private static OutfitCardItem ToOutfitItem(
        long itemId, IReadOnlyDictionary<long, Clothe> clothesById, IReadOnlyDictionary<string, string> imageUrls)
    {
        return new OutfitCardItem
        {
            Id = itemId,
            CategoryId = clothesById.TryGetValue(itemId, out var clothe) ? clothe.CategoryId ?? 0 : 0,
            ImageUrl = ImageUrlOf(itemId, clothesById, imageUrls),
        };
    }

    private static string? ImageUrlOf(
        long itemId, IReadOnlyDictionary<long, Clothe> clothesById, IReadOnlyDictionary<string, string> imageUrls)
    {
        if (!clothesById.TryGetValue(itemId, out var clothe) || string.IsNullOrEmpty(clothe.ImagePath))
            return null;
        return imageUrls.TryGetValue(clothe.ImagePath, out var imageUrl) ? imageUrl : null;
    }
}