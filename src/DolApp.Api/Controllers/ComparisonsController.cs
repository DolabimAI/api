using DolApp.Api.Middleware;
using DolApp.Application.UseCases.Comparisons.CreateSession;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DolApp.Api.Controllers;

/// Karşılaştırma uçları (/api/comparisons, C5–C8). Hepsi token ister (Program.cs'teki RequireAuthorization).
/// Hiçbir cevapta yöntem bilgisi (kural / öğrenen), left_method_id ya da sonuç değeri yoktur.
/// C5: sessions. C6 (select, reject-both), C7 (complete) ve C8 (swap) bu controller'a eklenecek.

[ApiController]
[Route("api/comparisons")]
public class ComparisonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ComparisonsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// Kombin isteği: formdaki bağlamla iki kart (ya da iki yöntem aynı kombini seçtiyse tek kart).
    /// Kullanıcının tepki bekleyen önceki kartı TEPKISIZ olur. Hata olursa hiçbir şey yazılmaz.
    [HttpPost("sessions")]
    [ProducesResponseType<CreateSessionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<CreateSessionResponse>> CreateSession(
        CreateSessionRequest request, CancellationToken cancellationToken)
    {
        var createSessionResponse = await _mediator.Send(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, createSessionResponse);
    }
}