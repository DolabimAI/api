using DolApp.Application.UseCases.Wardrobe.GetWardrobeStatus;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DolApp.Api.Controllers;

/// Dolap uçları. C4'te dolap durumu; C3'teki parça uçları ayrı bir controller'da (/api/clothes) olacak. Token ister.

[ApiController]
[Route("api/wardrobe")]
public class WardrobeController : ControllerBase
{
    private readonly IMediator _mediator;

    public WardrobeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("status")]
    public async Task<ActionResult<GetWardrobeStatusResponse>> GetWardrobeStatus(CancellationToken cancellationToken)
    {
        var getWardrobeStatusResponse = await _mediator.Send(new GetWardrobeStatusRequest(), cancellationToken);
        return Ok(getWardrobeStatusResponse);
    }
}