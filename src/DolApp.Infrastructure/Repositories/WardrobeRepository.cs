using DolApp.Application.Interfaces;
using DolApp.Core.Entities;
using DolApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DolApp.Infrastructure.Repositories;

public class WardrobeRepository : IWardrobeRepository
{
    private readonly AppDbContext _dbContext;

    public WardrobeRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WardrobeSummary?> GetSummaryByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        // Görünümde kullanıcı id'si yok, dolap id'si var; kullanıcının dolabı wardrobes tablosundan bulunur (tek sorgu).
        return await _dbContext.WardrobeSummaries
            .AsNoTracking()
            .Where(wardrobeSummary => _dbContext.Wardrobes.Any(
                wardrobe => wardrobe.Id == wardrobeSummary.WardrobeId && wardrobe.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken);
    }
}