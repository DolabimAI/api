using System.Text.Json.Serialization;
using MediatR;

namespace DolApp.Application.UseCases.Clothes.UpdateClothing;

/// PATCH /api/clothes/{clothingId} isteği: parça detayında etiket düzenleme (FR-09, S5).
/// Yalnızca gönderilen alanlar değişir; gönderilmeyen (null) alan olduğu gibi kalır. Örnek: { "toneId": 3 }
/// Tarz ya da mevsim gönderilirse boş olamaz (boş tarz parçayı bütün etkinliklerden düşürür).

public class UpdateClothingRequest : IRequest<UpdateClothingResponse>
{
    /// Adresten gelir (controller doldurur); gövdede okunmaz.
    [JsonIgnore]
    public long ClothingId { get; set; }

    public long? SubtypeId { get; set; }
    public long? MainColorId { get; set; }
    public long? ToneId { get; set; }
    public bool? IsPatterned { get; set; }
    public List<long>? StyleIds { get; set; }
    public List<long>? SeasonIds { get; set; }
    public long? ThicknessId { get; set; }
}