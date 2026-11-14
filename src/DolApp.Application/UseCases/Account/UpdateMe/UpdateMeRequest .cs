using System.Text.Json.Serialization;
using DolApp.Application.UseCases.Account.GetMe;
using MediatR;

namespace DolApp.Application.UseCases.Account.UpdateMe;

/// PATCH /api/me isteği. Yalnızca gönderilen alanlar güncellenir.
/// Ad ve soyad: null ya da gönderilmemiş = değiştirme.
/// Şehrim: il ya da ilçeden biri JSON'da yazılıysa (null olsa bile) Şehrim güncellenir;
/// ikisi de null = Şehrim'i temizle. Hiçbiri yazılmamışsa Şehrim'e dokunulmaz.

public class UpdateMeRequest : IRequest<GetMeResponse>
{
    private long? _defaultCityId;
    private long? _defaultDistrictId;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public long? DefaultCityId
    {
        get => _defaultCityId;
        set
        {
            _defaultCityId = value;
            IsDefaultLocationSent = true;
        }
    }

    public long? DefaultDistrictId
    {
        get => _defaultDistrictId;
        set
        {
            _defaultDistrictId = value;
            IsDefaultLocationSent = true;
        }
    }

    /// JSON'da il ya da ilçe alanı yazılı mıydı? JSON okuyucu set'i yalnızca alan yazılıysa çağırır.
    /// [JsonIgnore]: bu alan JSON'dan okunmaz ve Swagger'da görünmez; yalnızca sunucu içinde kullanılır.
    [JsonIgnore]
    public bool IsDefaultLocationSent { get; private set; }
}