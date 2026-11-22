using DolApp.Application.UseCases.Clothes.AnalyzeClothing;
using MediatR;

namespace DolApp.Application.UseCases.Clothes.GetClothingById;

/// GET /api/clothes/{clothingId} isteği: parça detayı (FR-09, S5). Gövdesizdir; id adresten gelir.
/// Cevap analyze ve onay cevabıyla aynı biçimdedir (AnalyzeClothingResponse); uygulama tek tip kullanır.

public class GetClothingByIdRequest : IRequest<GetClothingByIdResponse>
{
    public long ClothingId { get; set; }
}