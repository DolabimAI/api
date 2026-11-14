namespace DolApp.Core.Errors;

public static class ErrorMessages
{
    // Genel
    public const string InvalidRequest = "İstek biçimi hatalı.";
    public const string InternalError = "Beklenmeyen bir hata oluştu, biraz sonra tekrar dene.";
    public const string Unauthorized = "Oturumun sona erdi, tekrar giriş yap.";
}

/// Hata kodlarına karşılık gelen, ekranda gösterilecek Türkçe mesajlar.
/// Her kodun mesajı yalnızca burada yazılır; aynı koda her yerde aynı mesaj gider.
/// Sabitlerin adları ErrorCodes'takilerle aynıdır.