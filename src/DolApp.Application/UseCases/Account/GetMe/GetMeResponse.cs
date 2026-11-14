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
}