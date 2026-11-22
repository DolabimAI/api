using MediatR;

namespace DolApp.Application.UseCases.Clothes.GetClothes;

/// GET /api/clothes isteği: Dolabım listesi (FR-09, S4). ?subtypeId=5 verilirse yalnızca o tür döner.

public class GetClothesRequest : IRequest<GetClothesResponse>
{
    public long? SubtypeId { get; set; }
}