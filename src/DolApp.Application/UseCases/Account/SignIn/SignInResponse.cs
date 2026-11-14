namespace DolApp.Application.UseCases.Account.SignIn;

/// POST /api/account/signin cevabı (200). Kayıt cevabıyla aynı biçimdedir;
/// uygulama iki durumda da token'ı saklar ve IsOnboardingAccepted'a göre ekran seçer.

public class SignInResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public bool IsOnboardingAccepted { get; set; }
}