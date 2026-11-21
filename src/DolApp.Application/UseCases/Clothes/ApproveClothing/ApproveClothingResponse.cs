using DolApp.Application.UseCases.Clothes.AnalyzeClothing;
using DolApp.Application.UseCases.Wardrobe.GetWardrobeStatus;

namespace DolApp.Application.UseCases.Clothes.ApproveClothing;

/// POST /api/clothes/{clothingId}/approve cevabı (200). 17 Kasım kararı (API belgesi, C.9):
/// "clothing" analyze cevabıyla, "wardrobeStatus" GET /api/wardrobe/status cevabıyla aynı biçimde.
/// Uygulama eksik kartının kaybolup kaybolmayacağını wardrobeStatus'tan bilir.

public class ApproveClothingResponse
{
    public AnalyzeClothingResponse Clothing { get; set; } = new();
    public GetWardrobeStatusResponse WardrobeStatus { get; set; } = new();
}