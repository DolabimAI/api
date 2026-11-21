using DolApp.Application.Interfaces;
using DolApp.Core.Entities;
using DolApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DolApp.Infrastructure.Repositories;

public class ClothingRepository : IClothingRepository
{
    private readonly AppDbContext _dbContext;

    public ClothingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Clothe>> GetDraftsAsync(long wardrobeId, long draftStatusId, CancellationToken cancellationToken)
    {
        return await _dbContext.Clothes
            .Where(clothe => clothe.WardrobeId == wardrobeId && clothe.StatusId == draftStatusId)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(IReadOnlyCollection<Clothe> clothes, CancellationToken cancellationToken)
    {
        if (clothes.Count == 0)
            return;

        _dbContext.Clothes.RemoveRange(clothes);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddWithPredictionAsync(Clothe clothe, ModelPrediction modelPrediction, CancellationToken cancellationToken)
    {
        // Tahmin parçaya bağlanır; EF önce clothes satırını yazar, oluşan id'yi tahminin clothing_id'sine koyar.
        clothe.ModelPrediction = modelPrediction;
        _dbContext.Clothes.Add(clothe);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Clothe?> GetForUpdateAsync(long clothingId, long wardrobeId, CancellationToken cancellationToken)
    {
        // Dolap koşulu sahipliği sağlar: başkasının parçası "yok" gibi görünür.
        return await _dbContext.Clothes
            .Include(clothe => clothe.ModelPrediction)
            .FirstOrDefaultAsync(clothe => clothe.Id == clothingId && clothe.WardrobeId == wardrobeId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}