using DolApp.Application.UseCases.Account.GetMe;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DolApp.Api.Controllers;

/// Giriş yapmış kullanıcının kendi hesabıyla ilgili uçlar (/api/me).
/// [AllowAnonymous] yok: Program.cs'teki RequireAuthorization sayesinde hepsi token ister.

[ApiController]
[Route("api/me")]
public class MeController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<GetMeResponse>> GetMe(CancellationToken cancellationToken)
    {
        var getMeResponse = await _mediator.Send(new GetMeRequest(), cancellationToken);
        return Ok(getMeResponse);
    }
}