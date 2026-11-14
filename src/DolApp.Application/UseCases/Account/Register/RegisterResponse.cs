namespace DolApp.Application.UseCases.Account.Register;

/// POST /api/account/register cevabı (201). Giriş cevabıyla aynı biçimdedir.
/// IsOnboardingAccepted kayıtta her zaman false'tur: uygulama Bilgilendirme ekranını açar.

public class RegisterResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public bool IsOnboardingAccepted { get; set; }
}