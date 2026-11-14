namespace DolApp.Application.Interfaces;

/// Giriş hesabı işlemleri. Use case'ler hesabın nerede tutulduğunu (Supabase) bilmez;
/// implementasyon Infrastructure'dadır (SupabaseAuthClient).
/// Beklenen hatalar AppException olarak fırlatılır (email_taken, weak_password, invalid_credentials).
public interface IAuthService
{
    /// Doğrulanmış e-postayla yeni giriş hesabı açar ve hesabın id'sini döner.
    Task<Guid> CreateUserAsync(string email, string password, CancellationToken cancellationToken);

    /// Giriş hesabını siler. Hesap zaten yoksa hata vermez.
    Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken);

    /// E-posta ve şifreyi kontrol eder; doğruysa token döner. 
    Task<AuthSession> SignInAsync(string email, string password, CancellationToken cancellationToken);
}

/// Başarılı girişin sonucu: kullanıcı id'si, token ve token'ın bitiş zamanı.
public record AuthSession(Guid UserId, string AccessToken, DateTimeOffset ExpiresAt);