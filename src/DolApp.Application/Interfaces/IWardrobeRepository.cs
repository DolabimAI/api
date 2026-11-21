using DolApp.Core.Entities;

namespace DolApp.Application.Interfaces;

/// Kullanıcının dolabı ve parçaları. C4'te dolap özeti; C3'te parça işlemleri eklenecek.

public interface IWardrobeRepository
{
    Task<WardrobeSummary?> GetSummaryByUserIdAsync(Guid userId, CancellationToken cancellationToken);     /// Kullanıcının dolabındaki onaylı parça sayıları (wardrobe_summary görünümü).
    Task<long?> GetWardrobeIdByUserIdAsync(Guid userId, CancellationToken cancellationToken);     /// Kullanıcının dolabının id'si (kayıtta her kullanıcıya bir dolap açılır). Bulunamazsa null.

}