namespace DolApp.Core.Errors;

public static class ErrorCodes
{
    // Genel: middleware kullanır
    public const string InvalidRequest = "invalid_request";
    public const string InternalError = "internal_error";
}

/// API'nin dönebileceği hata kodları. Uygulama (Bilge, Sude) bu metinlere göre karar verir;
/// değerler API Uçları belgesiyle birebir aynı olmalı. Yeni kod, onu kullanan uç yazılırken eklenir