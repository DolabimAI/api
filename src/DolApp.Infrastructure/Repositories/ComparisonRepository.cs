using DolApp.Application.Interfaces;
using DolApp.Core.Entities;
using DolApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DolApp.Infrastructure.Repositories;

/// Karşılaştırma oturumları ve kartları. Ayrıntılar IComparisonRepository'de.

public class ComparisonRepository : IComparisonRepository
{
    private readonly AppDbContext _dbContext;

    public ComparisonRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Comparison>> GetPendingAsync(Guid userId, long pendingResultId, CancellationToken cancellationToken)
    {
        // Takipli: sonuç değiştirilip SaveChangesAsync ile yazılacak.
        return await _dbContext.Comparisons
            .Where(comparison => comparison.Session.UserId == userId && comparison.ResultId == pendingResultId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Comparison>> GetLearningHistoryAsync(
        Guid userId, List<long> learningResultIds, CancellationToken cancellationToken)
    {
        // Yalnızca okunacağı için takipsiz. completed_at'i boş karar sözleşmeye göre gönderilemez, alınmaz.
        return await _dbContext.Comparisons
            .AsNoTracking()
            .Include(comparison => comparison.Session)
            .Where(comparison => comparison.Session.UserId == userId
                && learningResultIds.Contains(comparison.ResultId)
                && comparison.CompletedAt != null)
            .OrderBy(comparison => comparison.CompletedAt)
            .ThenBy(comparison => comparison.Id)
            .ToListAsync(cancellationToken);
    }

    public void AddSession(ComparisonSession session)
    {
        // Oturumun Comparisons listesindeki kartlar da eklenir; EF önce oturumu yazar, kartlara session_id'yi kendisi verir.
        _dbContext.ComparisonSessions.Add(session);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}