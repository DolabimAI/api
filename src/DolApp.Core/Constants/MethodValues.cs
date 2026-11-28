namespace DolApp.Core.Constants;

/// methods tablosundaki value kodları (002_seed_lookups.sql). Kod içinde metin yerine bu sabitler kullanılır.
/// comparisons.left_method_id (soldaki kartın yöntemi) ve outfit_changes.method_id bu tabloya bağlıdır.

public static class MethodValues
{
    public const string Rule = "kural";
    public const string Learned = "ogrenen";

    /// İki yöntemin dışında, aksesuar seçen sabit kural (C8'de aksesuar değişikliğinde).
    public const string SharedRule = "ortak_kural";
}