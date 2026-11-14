using DolApp.Application.UseCases.Weather.GetWeather;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DolApp.Api.Controllers;

/// Hava özeti. Ana sayfa ve kombin formu kullanır. Token ister.

[ApiController]
[Route("api/weather")]
public class WeatherController : ControllerBase
{
    private readonly IMediator _mediator;

    public WeatherController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<GetWeatherResponse>> GetWeather(
        [FromQuery] long? cityId,
        [FromQuery] long? districtId,
        [FromQuery] DateOnly? date,
        [FromQuery] long? timeOfDayId,
        CancellationToken cancellationToken)
    {
        var getWeatherRequest = new GetWeatherRequest
        {
            CityId = cityId,
            DistrictId = districtId,
            Date = date,
            TimeOfDayId = timeOfDayId,
        };
        var getWeatherResponse = await _mediator.Send(getWeatherRequest, cancellationToken);
        return Ok(getWeatherResponse);
    }
}