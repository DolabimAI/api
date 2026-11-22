using System.Globalization;
using DolApp.Core.Entities;

namespace DolApp.Application.UseCases.Clothes.GetClothes;

/// Dolabım listesinin gruplarını kurar. Veritabanına ve depoya gitmez; bu yüzden birim testiyle denenir.
/// Kurallar (FR-09): yalnızca parçası olan türler; gruplar adına göre Türkçe alfabetik (Ç, Ş, İ doğru yerde);
/// grup içinde en son onaylanan önce.

public static class ClothingGroupBuilder
{
    private static readonly StringComparer TurkishComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), ignoreCase: false);

    public static List<ClothingGroup> Build(
        IEnumerable<Clothe> approvedClothes,
        IReadOnlyDictionary<long, string> subtypeNames,
        IReadOnlyDictionary<string, string> imageUrls,
        long? subtypeId)
    {
        return approvedClothes
            .Where(clothe => clothe.SubtypeId.HasValue)   // onaylı parçada tür hep dolu; yine de güvence
            .Where(clothe => subtypeId == null || clothe.SubtypeId == subtypeId)
            .GroupBy(clothe => clothe.SubtypeId!.Value)
            .Select(group => new ClothingGroup
            {
                SubtypeId = group.Key,
                Name = subtypeNames[group.Key],
                Count = group.Count(),
                Items = group
                    .OrderByDescending(clothe => clothe.ApprovedAt)
                    .ThenByDescending(clothe => clothe.Id)
                    .Select(clothe => new ClothingListItem
                    {
                        Id = clothe.Id,
                        ImageUrl = ImageUrlOf(clothe, imageUrls),
                    })
                    .ToList(),
            })
            .OrderBy(group => group.Name, TurkishComparer)
            .ToList();
    }

    private static string? ImageUrlOf(Clothe clothe, IReadOnlyDictionary<string, string> imageUrls)
    {
        if (string.IsNullOrEmpty(clothe.ImagePath))
            return null;

        return imageUrls.TryGetValue(clothe.ImagePath, out var imageUrl) ? imageUrl : null;
    }
}