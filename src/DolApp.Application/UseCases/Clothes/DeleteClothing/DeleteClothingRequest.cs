using MediatR;

namespace DolApp.Application.UseCases.Clothes.DeleteClothing;

/// DELETE /api/clothes/{clothingId} isteği: Parça detayında Sil (FR-09, S5). Gövdesizdir; id adresten gelir.
/// Taslak silmek için ayrı uç var: DELETE /api/clothes/{clothingId}/draft.

public class DeleteClothingRequest : IRequest<DeleteClothingResponse>
{
    public long ClothingId { get; set; }
}