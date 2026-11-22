using DolApp.Core.Entities;

namespace DolApp.Application.UseCases.Clothes.UpdateClothing;

/// PATCH /api/clothes/{clothingId} cevabı (200): düzenlenmiş parça, detay ucuyla aynı alanlar.
/// Sıcaklık aralığı yok (plan kuralı, T-18).

public class UpdateClothingResponse
{
    public long ClothingId { get; set; }

    /// Süreli görsel bağlantısı (1 saat). Görsel depoda bulunamazsa null.
    public string? ImageUrl { get; set; }

    public long? SubtypeId { get; set; }
    public long? CategoryId { get; set; }
    public long? MainColorId { get; set; }
    public long? ToneId { get; set; }
    public bool IsPatterned { get; set; }
    public List<long> StyleIds { get; set; } = new();
    public List<long> SeasonIds { get; set; } = new();
    public long? ThicknessId { get; set; }

    /// Parça satırından cevabı üretir.
    public static UpdateClothingResponse FromClothe(Clothe clothe, string? imageUrl)
    {
        return new UpdateClothingResponse
        {
            ClothingId = clothe.Id,
            ImageUrl = imageUrl,
            SubtypeId = clothe.SubtypeId,
            CategoryId = clothe.CategoryId,
            MainColorId = clothe.MainColorId,
            ToneId = clothe.ToneId,
            IsPatterned = clothe.IsPatterned,
            StyleIds = clothe.StyleIds?.ToList() ?? new List<long>(),
            SeasonIds = clothe.SeasonIds?.ToList() ?? new List<long>(),
            ThicknessId = clothe.ThicknessId,
        };
    }
}