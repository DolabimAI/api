using DolApp.Api.Middleware;
using DolApp.Application.UseCases.Clothes.AnalyzeClothing;
using DolApp.Application.UseCases.Clothes.ApproveClothing;
using DolApp.Application.UseCases.Clothes.DeleteClothing;
using DolApp.Application.UseCases.Clothes.DeleteDraft;
using DolApp.Application.UseCases.Clothes.GetClothes;
using DolApp.Application.UseCases.Clothes.GetClothingById;
using DolApp.Application.UseCases.Clothes.UpdateClothing;
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
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status502BadGateway)]
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



        /// Taslak parçayı onaylar (Kaydet). Gövde, etiket onay ekranındaki son değerlerdir.
        [HttpPost("{clothingId}/approve")]
        [ProducesResponseType<ApproveClothingResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApproveClothingResponse>> Approve(
            long clothingId, ApproveClothingRequest request, CancellationToken cancellationToken)
        {
            request.ClothingId = clothingId;   // id adresten; gövdedeki alan yok sayılır ([JsonIgnore])
            var approveClothingResponse = await _mediator.Send(request, cancellationToken);
            return Ok(approveClothingResponse);
        }

        /// Taslak parçayı tamamen siler (Yeniden çek, kaydetmeden çıkış). Gövdesiz; başarıda 204.
        [HttpDelete("{clothingId}/draft")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteDraft(long clothingId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteDraftRequest { ClothingId = clothingId }, cancellationToken);
            return NoContent();
        }


        /// Dolabım listesi: onaylı parçalar, alt türe göre gruplu ve Türkçe alfabetik. ?subtypeId=5 ile tek türe daralır.
        [HttpGet]
        [ProducesResponseType<GetClothesResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetClothesResponse>> GetClothes(
            [FromQuery] long? subtypeId, CancellationToken cancellationToken)
        {
            var getClothesResponse = await _mediator.Send(new GetClothesRequest { SubtypeId = subtypeId }, cancellationToken);
            return Ok(getClothesResponse);
        }


        /// Parça detayı. Yalnızca onaylı parça; taslak, silinmiş ya da başkasının parçası 404.
        [HttpGet("{clothingId}")]
        [ProducesResponseType<GetClothingByIdResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetClothingByIdResponse>> GetClothingById(long clothingId, CancellationToken cancellationToken)
        {
            var getClothingByIdResponse = await _mediator.Send(new GetClothingByIdRequest { ClothingId = clothingId }, cancellationToken);
            return Ok(getClothingByIdResponse);
        }



        /// Etiket düzenleme. Yalnızca gönderilen alanlar değişir; değişen her alan label_changes'a "sonradan" yazılır.
        [HttpPatch("{clothingId}")]
        [ProducesResponseType<UpdateClothingResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UpdateClothingResponse>> UpdateClothing(
            long clothingId, UpdateClothingRequest request, CancellationToken cancellationToken)
        {
            request.ClothingId = clothingId;   // id adresten; gövdedeki alan yok sayılır ([JsonIgnore])
            var updateClothingResponse = await _mediator.Send(request, cancellationToken);
            return Ok(updateClothingResponse);
        }


        /// Parçayı siler (durum silindi). Satır ve görsel kalır; cevapta güncel dolap durumu döner.
        /// Taslak için bu uç 404 verir; taslak DELETE /api/clothes/{clothingId}/draft ile silinir.
        [HttpDelete("{clothingId}")]
        [ProducesResponseType<DeleteClothingResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeleteClothingResponse>> DeleteClothing(long clothingId, CancellationToken cancellationToken)
        {
            var deleteClothingResponse = await _mediator.Send(new DeleteClothingRequest { ClothingId = clothingId }, cancellationToken);
            return Ok(deleteClothingResponse);
        }
    }
}
