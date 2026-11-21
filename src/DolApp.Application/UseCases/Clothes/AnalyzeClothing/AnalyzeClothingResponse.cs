using DolApp.Core.Entities;

namespace DolApp.Application.UseCases.Clothes.AnalyzeClothing
{
    /// POST /api/clothes/analyze cevabı (201). Onay cevabındaki "clothing" da bu biçimde (API belgesi, 17 Kasım kararı).
    /// Örnek: { "clothingId": 1287, "imageUrl": "https://...", "subtypeId": 2, "categoryId": 1, "mainColorId": 11,
    ///          "toneId": 1, "isPatterned": false, "styleIds": [2, 4], "seasonIds": [1, 3], "thicknessId": 1 }
    /// Güven 0,40'ın altındaysa subtypeId, categoryId ve thicknessId null; styleIds ve seasonIds boş liste.
    /// Sıcaklık aralığı hiçbir cevapta yok (plan kuralı).

    public class AnalyzeClothingResponse
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


        /// Parça satırından cevabı üretir. Onay ucu (adım 3) da kullanacak.
        public static AnalyzeClothingResponse FromClothe(Clothe clothe, string? imageUrl)
        {
            return new AnalyzeClothingResponse
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
}
