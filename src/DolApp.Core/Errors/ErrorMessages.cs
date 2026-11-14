namespace DolApp.Core.Errors;

/// Kullanıcıya gösterilen hata mesajları. ErrorCodes'taki her sabitin burada aynı adla bir karşılığı var.
/// Dil: yumuşatılmış "sen" dili ("Lütfen …", "… dener misin?"). Yeni mesajlar da bu dille yazılır.
/// Uygulama kararını code'a göre verir; mesaj metni değişince uygulamanın davranışı etkilenmez.

public static class ErrorMessages
{
    // Genel
    public const string InvalidRequest = "İsteğin işlenemedi. Lütfen tekrar dene.";
    public const string InternalError = "Beklenmeyen bir sorun oluştu. Biraz sonra tekrar dener misin?";
    public const string Unauthorized = "Oturumunun süresi doldu. Lütfen tekrar giriş yap.";

    // Hesap
    public const string EmailTaken = "Bu e-posta adresiyle açılmış bir hesap zaten var. Giriş yapmayı deneyebilirsin.";
    public const string InvalidCredentials = "E-posta adresin ya da şifren hatalı görünüyor. Kontrol edip tekrar dener misin?";
    public const string WeakPassword = "Şifren yeterince güçlü değil. Daha uzun bir şifre seçer misin?";
    public const string RequiredFields = "Lütfen zorunlu alanların hepsini doldur.";
    public const string InvalidEmail = "Lütfen geçerli bir e-posta adresi gir.";
    public const string Under18 = "DolabımAI'yi kullanabilmek için 18 yaşında ya da daha büyük olman gerekiyor.";
    public const string ConsentRequired = "Kayıt olabilmek için onam metnini okuyup onaylaman gerekiyor.";
}