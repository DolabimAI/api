namespace DolApp.Core.Errors;

public static class ErrorCodes
{
    public const string InvalidRequest = "invalid_request";
    public const string InternalError = "internal_error";
    public const string Unauthorized = "unauthorized";

    // Hesap oluşumu için gerekli olan hata kodları
    public const string EmailTaken = "email_taken";
    public const string InvalidCredentials = "invalid_credentials";
    public const string WeakPassword = "weak_password";
}

// Genel: middleware metodu kullanılır

