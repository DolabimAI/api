using MediatR;

namespace DolApp.Application.UseCases.Wardrobe.GetWardrobeStatus;

/// GET /api/wardrobe/status isteği. Alanı yoktur; dolap token'daki kullanıcıdan bulunur.

public class GetWardrobeStatusRequest : IRequest<GetWardrobeStatusResponse>
{
}