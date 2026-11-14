using MediatR;

namespace DolApp.Application.UseCases.Locations.GetDistricts;

/// GET /api/cities/{id}/districts isteği. CityId adresten (route) gelir; controller doldurur.

public class GetDistrictsRequest : IRequest<GetDistrictsResponse>
{
    public long CityId { get; set; }
}