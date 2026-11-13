namespace DolApp.Core.Errors;

public class AppException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }

    public AppException(int statusCode, string code, string message) : base(message)
    {
        StatusCode = statusCode;
        Code = code;
    }
}


/// Kullanıcıya {code, message} olarak dönecek, beklenen hatalar için kullanılır.
/// Örnek: throw new AppException(409, ErrorCodes.EmailTaken, "Bu e-posta ile kayıtlı bir hesap var.");
/// Beklenmeyen hatalar (bug, bağlantı kopması) bu sınıfla atılmaz; middleware onları 500'e çevirir.