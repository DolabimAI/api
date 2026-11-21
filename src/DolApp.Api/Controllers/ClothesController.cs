using DolApp.Application.UseCases.Clothes.AnalyzeClothing;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DolApp.Api.Controllers
{
    /// Parça uçları (/api/clothes, C3). Hepsi token ister (Program.cs'teki RequireAuthorization).
    /// Bu adımda yalnızca analyze; approve, taslak silme, liste, detay, düzenleme ve silme sonraki adımlarda eklenecek.

    [ApiController]
    [Route("api/clothes")]
    public class ClothesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClothesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// Fotoğrafı analiz eder ve taslak parça açar. Fotoğraf multipart/form-data olarak "image" alanında gelir
        /// (parametre adı alan adını belirler; Python sözleşmesiyle aynı ad).
        [HttpPost("analyze")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType<AnalyzeClothingResponse>(StatusCodes.Status201Created)]
        public async Task<ActionResult<AnalyzeClothingResponse>> Analyze(IFormFile? image, CancellationToken cancellationToken)
        {
            // Dosya yoksa akış da null kalır; validator 400 required_fields döner.
            await using var imageStream = image?.OpenReadStream();

            var analyzeClothingRequest = new AnalyzeClothingRequest
            {
                ImageStream = imageStream,
                FileName = image?.FileName ?? string.Empty,
                FileLength = image?.Length ?? 0,
            };
            var analyzeClothingResponse = await _mediator.Send(analyzeClothingRequest, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, analyzeClothingResponse);
        }
    }
}
