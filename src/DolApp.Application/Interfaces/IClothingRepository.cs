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
    }
}
