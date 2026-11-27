using DolApp.Core.Entities;

namespace DolApp.Application.Interfaces;

/// Karşılaştırma oturumları ve kartları (comparison_sessions, comparisons). C5 ile başlar; C6, C7 ve C10 da kullanacak.
/// Yazma kalıbı: metotlar yalnızca ekler ya da takipli satır döner; SaveChangesAsync hepsini tek seferde yazar (tek işlem).
/// Sahiplik oturum üzerinden sağlanır: yalnızca kullanıcının kendi oturumlarındaki kartlar döner.

public interface IComparisonRepository
{
    /// Kullanıcının tepki bekleyen (BEKLIYOR) kartları; normalde en fazla bir tane. Takipli döner:
    /// C5 yeni istekte bunları TEPKISIZ yapar ve yeni oturumla birlikte SaveChangesAsync ile yazar.
    Task<List<Comparison>> GetPendingAsync(Guid userId, long pendingResultId, CancellationToken cancellationToken);

    /// Öğrenmeye giren ve tamamlanmış (completed_at dolu) kararlar, kullanıcının bütün oturumlarından,
    /// eskiden yeniye (completed_at, eşitlikte id). Oturumla (karar anındaki bağlam) birlikte, takipsiz.
    Task<List<Comparison>> GetLearningHistoryAsync(
        Guid userId, List<long> learningResultIds, CancellationToken cancellationToken);

    /// Yeni oturumu ve Comparisons listesine eklenmiş kartları ekler; veritabanına yazmaz (SaveChangesAsync).
    void AddSession(ComparisonSession session);

    /// Takipteki bütün değişiklikleri (eklenen oturum ve kartlar, değişen sonuçlar) tek SaveChanges ile yazar = tek işlem.
    Task SaveChangesAsync(CancellationToken cancellationToken);
}