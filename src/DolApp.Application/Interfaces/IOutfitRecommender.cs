using DolApp.Application.Models;

namespace DolApp.Application.Interfaces;

/// Kombin önerisini Python'un /recommend ucundan alır (A3 sözleşmesi §6).
/// İki kart (Kural ve Öğrenen) tek çağrıda, aynı aday havuzundan gelir. Veritabanına C# yazar; Python yalnızca hesaplar.
/// Gerçek istemci PythonOutfitRecommender; ayarla FakeOutfitRecommender kullanılabilir (Python:UseFakeRecommender).

public interface IOutfitRecommender
{
    /// Aday yoksa hata fırlatmaz, HasCandidates = false döner: C5 bunu 422 no_candidates'e,
    /// C6 (İkisi de olmaz) { noMoreCandidates: true } cevabına çevirir.
    /// Python hata verirse, cevap sözleşmeye uymazsa ya da süre aşılırsa 502 recommendation_failed fırlatır.
    Task<OutfitRecommendationResult> RecommendAsync(OutfitRecommendationRequest request, CancellationToken cancellationToken);
}