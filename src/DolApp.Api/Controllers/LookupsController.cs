using DolApp.Application.UseCases.Lookups.GetLookups;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DolApp.Api.Controllers;

/// Uygulamanın açılır listeleri. Uygulama açılışta bir kez çağırır. Token ister.

[ApiController]
[Route("api/lookups")]
public class LookupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LookupsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<GetLookupsResponse>> GetLookups(CancellationToken cancellationToken)
    {
        var getLookupsResponse = await _mediator.Send(new GetLookupsRequest(), cancellationToken);
        return Ok(getLookupsResponse);
    }
}