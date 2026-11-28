namespace DolApp.Application.Models;

/// Başarılı girişin sonucu: kullanıcı id'si, token ve token'ın bitiş zamanı.
public record AuthSession(Guid UserId, string AccessToken, DateTimeOffset ExpiresAt);