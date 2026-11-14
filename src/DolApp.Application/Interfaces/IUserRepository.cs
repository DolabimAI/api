namespace DolApp.Application.Interfaces;

/// users tablosuyla ilgili işlemler. Implementasyonu Infrastructure'dadır (UserRepository).

public interface IUserRepository
{
    /// Kullanıcı users tablosunda var ve silinmemişse true döner.
    Task<bool> IsActiveAsync(Guid userId, CancellationToken cancellationToken);
}
