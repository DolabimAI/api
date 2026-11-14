namespace DolApp.Core.Errors;

public static class ErrorMessages
{
    // Genel
    public const string InvalidRequest = "İstek biçimi hatalı.";
    public const string InternalError = "Beklenmeyen bir hata oluştu, biraz sonra tekrar dene.";
    public const string Unauthorized = "Oturumun sona erdi, tekrar giriş yap.";

    // Hesap
    public const string EmailTaken = "Bu e-posta ile kayıtlı bir hesap var.";
    public const string InvalidCredentials = "E-posta ya da şifre hatalı.";
    public const string WeakPassword = "Şifre yeterince güçlü değil, daha uzun bir şifre seç.";
    public const string RequiredFields = "Zorunlu alanları doldur.";
    public const string InvalidEmail = "Geçerli bir e-posta adresi gir.";
    public const string Under18 = "DolabımAI'yi kullanmak için 18 yaşında ya da daha büyük olmalısın.";
    public const string ConsentRequired = "Kayıt olmak için onam metnini okuyup kabul etmelisin.";
}

/// Hata kodlarına karşılık gelen, ekranda gösterilecek Türkçe mesajlar.
/// Her kodun mesajı yalnızca burada yazılır; aynı koda her yerde aynı mesaj gider.
/// Sabitlerin adları ErrorCodes'takilerle aynıdır.