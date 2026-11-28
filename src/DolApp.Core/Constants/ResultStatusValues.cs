namespace DolApp.Core.Constants;

/// result_statuses tablosundaki kodlar (002_seed_lookups.sql). Kod içinde metin yerine bu sabitler kullanılır.
/// Value tam ad (comparisons.result_id'nin işaret ettiği satırı bulmak için), ResultKey iki harfli kısa kod
/// (Python'a giden geçmişte "resultKey" olarak kullanılır). Seed'deki kodlar değişirse burası da güncellenmeli.

public static class ResultStatusValues
{
    // Kart gösterildi, tepki yok
    public const string Pending = "BEKLIYOR";

    // Kart seçildi, son karar bekleniyor (C6)
    public const string RuleSelected = "KURAL_SECILDI";
    public const string LearnedSelected = "OGRENEN_SECILDI";

    // Son karar (C7)
    public const string RuleClean = "KURAL_TEMIZ";
    public const string RuleWithChanges = "KURAL_DEGISIKLIKLE";
    public const string RuleRejected = "KURAL_RED";
    public const string LearnedClean = "OGRENEN_TEMIZ";
    public const string LearnedWithChanges = "OGRENEN_DEGISIKLIKLE";
    public const string LearnedRejected = "OGRENEN_RED";
    public const string SameClean = "AYNI_TEMIZ";
    public const string SameWithChanges = "AYNI_DEGISIKLIKLE";
    public const string SameRejected = "AYNI_RED";

    // İkisi de olmaz (C6)
    public const string Neither = "HICBIRI";

    // Tepki verilmeden yeni istek yapıldı (C5)
    public const string Unanswered = "TEPKISIZ";

    /// Öğrenen yönteme giren kararların kısa kodları (A3 sözleşmesi §6, history kuralları).
    /// Python'a giden geçmişte yalnızca bunlar gönderilir; diğerlerini Python reddeder.
    public static readonly IReadOnlyList<string> LearningResultKeys = new[] { "KT", "KD", "KR", "OT", "OD", "OR", "HB" };
}