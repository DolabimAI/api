using MediatR;

namespace DolApp.Application.UseCases.Clothes.DeleteDraft;

/// DELETE /api/clothes/{clothingId}/draft isteği: kaydetmeden çıkış ve Yeniden çek. Gövdesizdir; id adresten gelir.

public class DeleteDraftRequest : IRequest<DeleteDraftResponse>
{
    public long ClothingId { get; set; }
}