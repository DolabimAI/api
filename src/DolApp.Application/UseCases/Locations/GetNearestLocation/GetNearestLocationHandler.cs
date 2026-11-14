using DolApp.Application.Interfaces;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Locations.GetNearestLocation;

/// Koordinata en yakın ilçeyi ve ilini döner. Konum hiçbir yere yazılmaz; yalnızca hesap için kullanılır.

public class GetNearestLocationHandler : IRequestHandler<GetNearestLocationRequest, GetNearestLocationResponse>
{
    private readonly ILocationRepository _locationRepository;

    public GetNearestLocationHandler(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<GetNearestLocationResponse> Handle(
        GetNearestLocationRequest request, CancellationToken cancellationToken)
    {
        // Validator iki değerin dolu olduğunu garanti eder.
        var nearestDistrict = await _locationRepository.GetNearestDistrictAsync(
            request.Latitude!.Value, request.Longitude!.Value, cancellationToken);

        // districts tablosu boş değilse buraya hiç gelinmez; yine de açık bir cevap dönsün.
        if (nearestDistrict is null)
            throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);

        return new GetNearestLocationResponse
        {
            City = new LocationItem { Id = nearestDistrict.City.Id, Name = nearestDistrict.City.DisplayName },
            District = new LocationItem { Id = nearestDistrict.Id, Name = nearestDistrict.DisplayName },
        };
    }
}