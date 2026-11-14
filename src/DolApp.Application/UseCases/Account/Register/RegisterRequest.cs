using DolApp.Application.UseCases.Account.Register;
using MediatR;

/// POST /api/account/register isteğinin gövdesi (API Uçları belgesindeki alanlar).
/// IRequest<RegisterResponse>: MediatR'a "bu isteğin cevabı RegisterResponse'tur" der.
/// Metin alanlarının başlangıç değeri string.Empty: JSON'da alan hiç gönderilmezse boş metin olarak kalır ve validator "required_fields" hatasını verir.

public class RegisterRequest : IRequest<RegisterResponse>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string Password { get; set; } = string.Empty;
    public bool ConsentAccepted { get; set; }
}