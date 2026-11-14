using DolApp.Application.Interfaces;

namespace DolApp.Tests.Fakes;

/// IAuthService'in test için sahte hali: Supabase'e gitmez.
/// Hangi adımın hata vereceği test içinde ayarlanır; silinen hesaplar DeletedUserIds listesinde tutulur.

public class FakeAuthService : IAuthService
{
    public Guid CreatedUserId { get; } = Guid.NewGuid();

    /// Doluysa ilgili metot bu hatayı verir.
    public Exception? CreateUserException { get; set; }
    public Exception? SignInException { get; set; }

    /// DeleteUserAsync hangi id'lerle çağrıldı?
    public List<Guid> DeletedUserIds { get; } = new();

    public Task<Guid> CreateUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        if (CreateUserException is not null)
            return Task.FromException<Guid>(CreateUserException);

        return Task.FromResult(CreatedUserId);
    }

    public Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        DeletedUserIds.Add(userId);
        return Task.CompletedTask;
    }

    public Task<AuthSession> SignInAsync(string email, string password, CancellationToken cancellationToken)
    {
        if (SignInException is not null)
            return Task.FromException<AuthSession>(SignInException);

        var authSession = new AuthSession(
            CreatedUserId,
            "sahte-token",
            new DateTimeOffset(2026, 11, 21, 12, 0, 0, TimeSpan.Zero));
        return Task.FromResult(authSession);
    }
}