using DolApp.Application.Interfaces;
using DolApp.Core.Entities;
using MediatR;

namespace DolApp.Application.UseCases.Account.Register;

/// Kayıt akışı:
///   1) Supabase'te giriş hesabı açılır (yönetici yoluyla, e-posta doğrulamasız).
///   2) Bu hesaba giriş yapılıp token alınır.
///   3) users ve wardrobes satırları tek işlemde yazılır.
/// 2. ya da 3. adım başarısız olursa 1. adımda açılan giriş hesabı silinir; yarım hesap kalmaz (T-03).

public class RegisterHandler : IRequestHandler<RegisterRequest, RegisterResponse>
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RegisterHandler(
        IAuthService authService,
        IUserRepository userRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _authService = authService;
        _userRepository = userRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<RegisterResponse> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        // E-postayı tek biçimde sakla: "Ayse@Ornek.com" ile "ayse@ornek.com" aynı hesap sayılsın.
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // 1) Giriş hesabı. E-posta kayıtlıysa burada 409 email_taken fırlar; geri alınacak bir şey yoktur.
        var newUserId = await _authService.CreateUserAsync(normalizedEmail, request.Password, cancellationToken);

        try
        {
            // 2) Token
            var authSession = await _authService.SignInAsync(normalizedEmail, request.Password, cancellationToken);

            // 3) users + wardrobes (tek işlem)
            var newUser = new User
            {
                Id = newUserId,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = normalizedEmail,
                BirthDate = request.BirthDate,
                IsConsentAccepted = true,
                ConsentAt = _dateTimeProvider.UtcNow,
            };
            await _userRepository.AddWithWardrobeAsync(newUser, cancellationToken);

            return new RegisterResponse
            {
                AccessToken = authSession.AccessToken,
                ExpiresAt = authSession.ExpiresAt,
                FirstName = newUser.FirstName,
                IsOnboardingAccepted = false,
            };
        }
        catch
        {
            // Geri alma: 1. adımda açılan giriş hesabını sil.
            // CancellationToken.None: istek iptal edilmiş olsa bile silme işlemi yarıda kesilmesin.
            await _authService.DeleteUserAsync(newUserId, CancellationToken.None);
            throw;   // asıl hatayı olduğu gibi yukarı ilet (middleware 500'e çevirir)
        }
    }
}