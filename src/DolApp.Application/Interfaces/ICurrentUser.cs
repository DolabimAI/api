namespace DolApp.Application.Interfaces;

/// Bu isteği yapan kullanıcının kimliği. Token'daki "sub" alanından okunur;
/// uygulama kullanıcı id'si göndermez. Yalnızca token isteyen uçlarda kullanılır.

public interface ICurrentUser
{
    Guid UserId { get; }
}
