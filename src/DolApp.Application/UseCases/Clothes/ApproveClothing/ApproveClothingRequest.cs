using System.Text.Json.Serialization;
using MediatR;

namespace DolApp.Application.UseCases.Clothes.ApproveClothing;

/// POST /api/clothes/{clothingId}/approve isteği (Kaydet). Gövde, etiket onay ekranındaki son değerlerdir.
/// Örnek: { "subtypeId": 3, "mainColorId": 11, "toneId": 1, "isPatterned": false,
///          "styleIds": [2, 4], "seasonIds": [1, 3], "thicknessId": 1 }
/// Tür, ana renk ve ton zorunlu. Tarz, mevsim ya da kalınlık gönderilmezse (ya da liste boşsa) türün varsayılanı kullanılır.

public class ApproveClothingRequest : IRequest<ApproveClothingResponse>
{
    /// Adresten gelir (controller doldurur); gövdede okunmaz.
    [JsonIgnore]
    public long ClothingId { get; set; }

    public long? SubtypeId { get; set; }
    public long? MainColorId { get; set; }
    public long? ToneId { get; set; }
    public bool IsPatterned { get; set; }
    public List<long>? StyleIds { get; set; }
    public List<long>? SeasonIds { get; set; }
    public long? ThicknessId { get; set; }
}