using MediatR;

namespace DolApp.Application.UseCases.Locations.GetCities;

/// GET /api/cities isteği. Alanı yoktur.

public class GetCitiesRequest : IRequest<GetCitiesResponse>
{
}