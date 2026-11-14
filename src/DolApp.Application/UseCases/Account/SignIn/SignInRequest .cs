using MediatR;

namespace DolApp.Application.UseCases.Account.SignIn;

/// POST /api/account/signin isteğinin gövdesi.

public class SignInRequest : IRequest<SignInResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}