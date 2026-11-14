using DolApp.Application.Interfaces;

namespace DolApp.Api.Auth;

/// ICurrentUser'ın implementasyonu: kullanıcı id'sini gelen isteğin token'ındaki "sub" alanından okur.
/// Web isteğine bağlı olduğu için Infrastructure'da değil, Api'de durur.

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdText = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

            if (Guid.TryParse(userIdText, out var userId))
                return userId;

            // Buraya gelinirse, token istemeyen ([AllowAnonymous]) bir uçta ICurrentUser kullanılmıştır: kod hatası.
            throw new InvalidOperationException("İstekte kullanıcı kimliği yok.");
        }
    }
}
