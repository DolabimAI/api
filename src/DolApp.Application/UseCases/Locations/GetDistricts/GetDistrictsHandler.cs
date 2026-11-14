using System.Globalization;
using DolApp.Application.Interfaces;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Locations.GetDistricts;

/// Bir ilin ilçelerini Türkçe alfabeye göre sıralı döner. İl yoksa 404 not_found.

public class GetDistrictsHandler : IRequestHandler<GetDistrictsRequest, GetDistrictsResponse>
{
    private static readonly StringComparer TurkishComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), ignoreCase: false);

    private readonly ILocationRepository _locationRepository;

    public GetDistrictsHandler(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<GetDistrictsResponse> Handle(GetDistrictsRequest request, CancellationToken cancellationToken)
    {
        // Boş liste "il var ama ilçesi yok" ile "il yok" arasında ayrım yapmaz; bu yüzden önce il kontrol edilir.
        var cityExists = await _locationRepository.CityExistsAsync(request.CityId, cancellationToken);
        if (!cityExists)
            throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);

        var districts = await _locationRepository.GetDistrictsByCityIdAsync(request.CityId, cancellationToken);

        return new GetDistrictsResponse
        {
            Districts = districts
                .OrderBy(district => district.DisplayName, TurkishComparer)
                .Select(district => new DistrictItem
                {
                    Id = district.Id,
                    Name = district.DisplayName,
                })
                .ToList(),
        };
    }
}