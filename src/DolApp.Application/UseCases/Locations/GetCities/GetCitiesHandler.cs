using System.Globalization;
using DolApp.Application.Interfaces;
using MediatR;

namespace DolApp.Application.UseCases.Locations.GetCities;

/// 81 ili Türkçe alfabeye göre sıralı döner (Ç, Ğ, İ, Ö, Ş, Ü doğru yerde).

public class GetCitiesHandler : IRequestHandler<GetCitiesRequest, GetCitiesResponse>
{
    private static readonly StringComparer TurkishComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), ignoreCase: false);

    private readonly ILocationRepository _locationRepository;

    public GetCitiesHandler(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<GetCitiesResponse> Handle(GetCitiesRequest request, CancellationToken cancellationToken)
    {
        var cities = await _locationRepository.GetCitiesAsync(cancellationToken);

        return new GetCitiesResponse
        {
            Cities = cities
                .OrderBy(city => city.DisplayName, TurkishComparer)
                .Select(city => new CityItem
                {
                    Id = city.Id,
                    Name = city.DisplayName,
                    PlateCode = city.PlateCode,
                })
                .ToList(),
        };
    }
}