using DolApp.Core.Entities;

namespace DolApp.Application.Interfaces;

/// users tablosuyla ilgili işlemler. Implementasyonu Infrastructure'dadır (UserRepository).

public interface IUserRepository
{
    /// Kullanıcı users tablosunda var ve silinmemişse true döner.
    Task<bool> IsActiveAsync(Guid userId, CancellationToken cancellationToken);

    /// Yeni kullanıcıyı ve dolabını tek işlemde yazar: ikisi birlikte yazılır ya da hiçbiri yazılmaz.
    Task AddWithWardrobeAsync(User user, CancellationToken cancellationToken);

    /// Silinmemiş kullanıcıyı getirir; yoksa ya da silinmişse null döner.
    Task<User?> GetActiveByIdAsync(Guid userId, CancellationToken cancellationToken);

}
