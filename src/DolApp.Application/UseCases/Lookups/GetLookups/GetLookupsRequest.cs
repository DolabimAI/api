using MediatR;

namespace DolApp.Application.UseCases.Lookups.GetLookups;

/// GET /api/lookups isteği. Alanı yoktur.

public class GetLookupsRequest : IRequest<GetLookupsResponse>
{

}