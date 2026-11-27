namespace DolApp.Infrastructure.Clients
{
    public class PythonOptions
    {
        /// Python servisinin adresi (ör. http://localhost:8000). Sonunda / olmadan.
        public required string BaseUrl { get; init; }

        /// /analyze için bekleme süresi; aşılırsa 502 analysis_failed (plan kararı: 30 sn, pilotta ayarlanır).
        public int TimeoutSeconds { get; init; } = 30;

        /// true ise Python'a gidilmez; FakeClothingAnalyzer sözleşmedeki örnek cevabı döner.
        public bool UseFake { get; init; }

        /// Taklit cevaptaki güven. 0,40'ın altına çekilirse "tür boş" durumu denenir (T-10'un C# kısmı).
        public decimal FakeConfidence { get; init; } = 0.873m;

        /// true ise /recommend için Python'a gidilmez; FakeOutfitRecommender kullanıcının kendi parçalarından kombin kurar.
        /// UseFake'ten bağımsızdır: analiz taklit, öneri gerçek (ya da tersi) denenebilir.
        public bool UseFakeRecommender { get; init; }
    }
}
