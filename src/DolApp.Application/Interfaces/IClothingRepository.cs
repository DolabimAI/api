using DolApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DolApp.Application.Interfaces
{
    /// Parçalar (clothes) ve modelin tahmini (model_predictions). C3'ün uçları kullanır; her uçla birlikte büyüyecek.

    public interface IClothingRepository
    {
        Task<List<Clothe>> GetDraftsAsync(long wardrobeId, long draftStatusId, CancellationToken cancellationToken);         /// Dolaptaki taslak parçalar (normalde en fazla bir tane; uygulama çöktüyse kalmış olabilir). Takipli döner, silinebilir.
        Task DeleteAsync(IReadOnlyCollection<Clothe> clothes, CancellationToken cancellationToken);         /// Taslakları siler. Tahminleri veritabanındaki "on delete cascade" ile birlikte silinir. Tek SaveChanges = tek işlem.
        Task AddWithPredictionAsync(Clothe clothe, ModelPrediction modelPrediction, CancellationToken cancellationToken);          /// Yeni taslak parçayı ve modelin tahminini birlikte yazar. Tek SaveChanges = tek işlem: ya ikisi de yazılır ya hiçbiri.

       /// Kullanıcının dolabındaki bir parça, modelin tahminiyle birlikte. Takipli döner (değiştirilip kaydedilebilir).
       /// Parça yoksa ya da başka bir kullanıcının dolabındaysa null; uç 404 döner (sahiplik kuralı, T-17).
        Task<Clothe?> GetForUpdateAsync(long clothingId, long wardrobeId, CancellationToken cancellationToken);
        Task<List<Clothe>> GetApprovedAsync(long wardrobeId, long approvedStatusId, CancellationToken cancellationToken);         /// Dolaptaki onaylı parçalar (Dolabım listesi). Takipsiz döner; yalnızca okunur, değiştirilmez.
        Task SaveChangesAsync(CancellationToken cancellationToken);        /// Takipteki değişiklikleri (parça ve ona eklenen label_changes satırları) tek SaveChanges ile yazar = tek işlem.

         /// Verilen id'lerdeki parçalar, durumu ne olursa olsun (onaylı, silinmiş), yalnızca bu dolaptan. Takipsiz.
        /// C5: geçmiş kararlardaki parçalar güncel etiketleriyle Python'a gider; sonradan silinmiş olanlar da dahil.
        Task<List<Clothe>> GetByIdsAsync(long wardrobeId, List<long> clothingIds, CancellationToken cancellationToken);
    }
}
