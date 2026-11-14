using DolApp.Application.Interfaces;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Account.SignIn;

/// Giriş akışı:
///   1) Supabase'e e-posta ve şifreyle giriş yapılır, token alınır (yanlışsa 401 invalid_credentials).
///   2) users tablosundan ad ve Bilgilendirme durumu okunur.
/// users satırı yoksa ya da hesap silinmişse de aynı 401 döner:
/// hangi e-postanın kayıtlı olduğu dışarıdan anlaşılmasın (T-05).

public class SignInHandler : IRequestHandler<SignInRequest, SignInResponse>
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;

    public SignInHandler(IAuthService authService, IUserRepository userRepository)
    {
        _authService = authService;
        _userRepository = userRepository;
    }

    public async Task<SignInResponse> Handle(SignInRequest request, CancellationToken cancellationToken)
    {
        // Kayıttaki ile aynı biçim: "Test1@Dolapp.test" ile de giriş yapılabilsin.
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // 1) Token
        var authSession = await _authService.SignInAsync(normalizedEmail, request.Password, cancellationToken);

        // 2) Kullanıcı bilgisi
        var activeUser = await _userRepository.GetActiveByIdAsync(authSession.UserId, cancellationToken);
        if (activeUser is null)
            throw new AppException(401, ErrorCodes.InvalidCredentials, ErrorMessages.InvalidCredentials);

        return new SignInResponse
        {
            AccessToken = authSession.AccessToken,
            ExpiresAt = authSession.ExpiresAt,
            FirstName = activeUser.FirstName ?? string.Empty,
            IsOnboardingAccepted = activeUser.IsOnboardingAccepted,
        };
    }
}