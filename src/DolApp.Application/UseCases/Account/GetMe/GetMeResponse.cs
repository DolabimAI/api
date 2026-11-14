using DolApp.Core.Entities;

namespace DolApp.Application.UseCases.Account.GetMe;

/// GET /api/me cevabı (200). PATCH /api/me de aynı biçimi döner.
/// Şehrim seçilmemişse DefaultCity ve DefaultDistrict null gelir; ilçe seçilmemişse yalnızca DefaultDistrict null olur.

public class GetMeResponse
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public bool IsOnboardingAccepted { get; set; }
    public MeLocationResponse? DefaultCity { get; set; }
    public MeLocationResponse? DefaultDistrict { get; set; }


    /// Kullanıcı satırını cevaba çevirir. DefaultCity ve DefaultDistrict Include ile yüklenmiş olmalıdır.
    public static GetMeResponse FromUser(User user)
    {
        return new GetMeResponse
        {
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            BirthDate = user.BirthDate,
            IsOnboardingAccepted = user.IsOnboardingAccepted,
            DefaultCity = user.DefaultCity is null
                ? null
                : new MeLocationResponse { Id = user.DefaultCity.Id, Name = user.DefaultCity.DisplayName },
            DefaultDistrict = user.DefaultDistrict is null
                ? null
                : new MeLocationResponse { Id = user.DefaultDistrict.Id, Name = user.DefaultDistrict.DisplayName },
        };
    }
}

