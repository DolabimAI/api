using DolApp.Application.Interfaces;
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
}