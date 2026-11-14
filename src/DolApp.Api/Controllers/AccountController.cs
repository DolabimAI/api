using DolApp.Application.UseCases.Account.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DolApp.Application.UseCases.Account.SignIn;

namespace DolApp.Api.Controllers;

/// Hesap açma ve giriş uçları. İkisi de token istemez ([AllowAnonymous]).

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(
        RegisterRequest request, CancellationToken cancellationToken)
    {
        var registerResponse = await _mediator.Send(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, registerResponse);
    }

    [AllowAnonymous]
    [HttpPost("signin")]
    public async Task<ActionResult<SignInResponse>> SignIn(
    SignInRequest request, CancellationToken cancellationToken)
    {
        var signInResponse = await _mediator.Send(request, cancellationToken);
        return Ok(signInResponse);
    }
}