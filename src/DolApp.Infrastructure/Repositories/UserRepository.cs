using DolApp.Application.Interfaces;
using DolApp.Core.Entities;
using DolApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DolApp.Infrastructure.Repositories;

/// IUserRepository'nin implementasyonu: users tablosu sorguları.

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> IsActiveAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.Users.AnyAsync(
            user => user.Id == userId && !user.IsDeleted,
            cancellationToken);
    }

    public async Task AddWithWardrobeAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Add(user);
        _dbContext.Wardrobes.Add(new Wardrobe { UserId = user.Id });

        // Tek SaveChanges = tek veritabanı işlemi: iki satır birlikte yazılır ya da hiçbiri yazılmaz.
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> GetActiveByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == userId && !user.IsDeleted, cancellationToken);
    }

    public async Task<User?> GetActiveWithDefaultLocationAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Include(user => user.DefaultCity)
            .Include(user => user.DefaultDistrict)
            .FirstOrDefaultAsync(user => user.Id == userId && !user.IsDeleted, cancellationToken);
    }
}