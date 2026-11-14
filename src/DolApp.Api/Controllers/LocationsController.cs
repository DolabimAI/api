using DolApp.Application.UseCases.Locations.GetCities;
using DolApp.Application.UseCases.Locations.GetDistricts;
using DolApp.Application.UseCases.Locations.GetNearestLocation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DolApp.Api.Controllers;

/// İl, ilçe ve konum uçları. Adresler farklı köklerde olduğu için (/api/cities, /api/locations)
/// controller'a ortak bir [Route] verilmez; her uç kendi tam adresini yazar.
/// Hepsi token ister (Program.cs'teki RequireAuthorization).

[ApiController]
public class LocationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LocationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("api/cities")]
    public async Task<ActionResult<List<CityItem>>> GetCities(CancellationToken cancellationToken)
    {
        var getCitiesResponse = await _mediator.Send(new GetCitiesRequest(), cancellationToken);
        return Ok(getCitiesResponse.Cities);
    }

    [HttpGet("api/cities/{cityId:long}/districts")]
    public async Task<ActionResult<List<DistrictItem>>> GetDistricts(long cityId, CancellationToken cancellationToken)
    {
        var getDistrictsResponse = await _mediator.Send(new GetDistrictsRequest { CityId = cityId }, cancellationToken);
        return Ok(getDistrictsResponse.Districts);
    }

    /// Sorgu parametre adları API belgesindeki gibi kısa (lat, lon); kodda açık adlarla (latitude, longitude) kullanılır.
    [HttpGet("api/locations/nearest")]
    public async Task<ActionResult<GetNearestLocationResponse>> GetNearestLocation(
        [FromQuery(Name = "lat")] decimal? latitude,
        [FromQuery(Name = "lon")] decimal? longitude,
        CancellationToken cancellationToken)
    {
        var getNearestLocationRequest = new GetNearestLocationRequest { Latitude = latitude, Longitude = longitude };
        var getNearestLocationResponse = await _mediator.Send(getNearestLocationRequest, cancellationToken);
        return Ok(getNearestLocationResponse);
    }
}