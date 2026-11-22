using DolApp.Application.UseCases.Wardrobe.GetWardrobeStatus;

namespace DolApp.Application.UseCases.Clothes.DeleteClothing;

/// DELETE /api/clothes/{clothingId} cevabı (200): silmeden sonraki dolap durumu.
/// "wardrobeStatus" GET /api/wardrobe/status cevabıyla aynı biçimdedir; yapı onay cevabındakiyle aynı (22 Kasım kararı).
/// Şart bozulduysa uygulama Ana sayfadaki eksik kartını geri getirir.

public class DeleteClothingResponse
{
    public GetWardrobeStatusResponse WardrobeStatus { get; set; } = new();
}