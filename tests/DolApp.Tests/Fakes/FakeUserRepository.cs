using DolApp.Application.Interfaces;
using DolApp.Core.Entities;

namespace DolApp.Tests.Fakes;

/// IUserRepository'nin test için sahte hali: veritabanına gitmez.
/// Yazılan kullanıcılar AddedUsers listesinde tutulur. Kayıt testinde kullanılmayan metotlar hata verir;
/// böylece handler beklenmedik bir metodu çağırırsa test bunu hemen gösterir.

public class FakeUserRepository : IUserRepository
{
    /// Doluysa AddWithWardrobeAsync bu hatayı verir (veritabanı yazımı başarısız).
    public Exception? AddWithWardrobeException { get; set; }

    public List<User> AddedUsers { get; } = new();

    public Task AddWithWardrobeAsync(User user, CancellationToken cancellationToken)
    {
        if (AddWithWardrobeException is not null)
            return Task.FromException(AddWithWardrobeException);

        AddedUsers.Add(user);
        return Task.CompletedTask;
    }

    public Task<bool> IsActiveAsync(Guid userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Bu testte kullanılmıyor.");
    }

    public Task<User?> GetActiveByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Bu testte kullanılmıyor.");
    }

    public Task<User?> GetActiveWithDefaultLocationAsync(Guid userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Bu testte kullanılmıyor.");
    }

    public Task<User?> GetActiveForUpdateAsync(Guid userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Bu testte kullanılmıyor.");
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Bu testte kullanılmıyor.");
    }
}