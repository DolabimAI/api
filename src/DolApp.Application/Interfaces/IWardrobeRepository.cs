using DolApp.Core.Entities;

namespace DolApp.Application.Interfaces;

/// Kullanıcının dolabı ve parçaları. C4'te dolap özeti; C3'te parça işlemleri eklenecek.

public interface IWardrobeRepository
{
    /// Kullanıcının dolabındaki onaylı parça sayıları (wardrobe_summary görünümü).
    Task<WardrobeSummary?> GetSummaryByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}