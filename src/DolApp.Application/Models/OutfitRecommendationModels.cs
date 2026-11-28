namespace DolApp.Application.Models;

// /recommend isteğinin ve cevabının sınıfları (A3 sözleşmesi, contracts/python-api.md §2, §3, §6).
// İstek sınıfları JSON'a olduğu gibi çevrilir (camelCase). Python bilinmeyen alanı ve yanlış türü reddeder (422),
// bu yüzden isteğe sözleşmede olmayan bir özellik EKLENMEZ. Liste değerleri kodla gider (gomlek, beyaz, gunluk);
// id ↔ kod çevirisini handler yapar.

/// İsteğin tamamı.

public class OutfitRecommendationRequest
{
    /// Öğrenen yöntem yalnızca bu kullanıcının geçmişinden öğrenir.
    public Guid UserId { get; set; }

    public RecommendationContext Context { get; set; } = new();

    /// Kullanıcının onaylı bütün parçaları, aksesuarlar dahil. Dolap şartı C#'ta önceden kontrol edilmiş olmalı.
    public List<RecommendationItem> Wardrobe { get; set; } = new();

    /// Yalnızca bu oturumda "İkisi de olmaz" ile reddedilen kombinler. Her biri ana parça id'leri
    /// (dış giyim dahil, aksesuar hariç). İlk istekte boş; geçmiş oturumların redleri eklenmez.
    public List<List<long>> RejectedOutfits { get; set; } = new();

    /// Öğrenmeye giren bütün kararlar (KT, KD, KR, OT, OD, OR, HB), ilk günden itibaren, eskiden yeniye.
    public List<RecommendationHistoryEntry> History { get; set; } = new();
}

/// Bağlam nesnesi (§3): kombinin hangi koşullar için istendiği.

public class RecommendationContext
{
    /// Vaktin ortalama hissedilen sıcaklığı, 1 ondalık (hava servisinden).
    public decimal FeelsLikeTemp { get; set; }

    /// weather_conditions.value (ör. parcali_bulutlu).
    public string WeatherCondition { get; set; } = string.Empty;

    /// times_of_day.value: sabah, ogle, aksam, gece.
    public string TimeOfDay { get; set; } = string.Empty;

    public bool IsDaytime { get; set; }

    /// Formda seçilen etkinlik (styles.value).
    public string EventStyle { get; set; } = string.Empty;

    public bool WantsAccessories { get; set; }
}

/// Parça nesnesi (§2). Etiketler kıyafetin GÜNCEL değerleri; ıslak/sıcak işaretleri alt türden.

public class RecommendationItem
{
    public long Id { get; set; }
    public string Subtype { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public bool IsPatterned { get; set; }

    /// Kullanıcının seçtiği tarzlar (alt türün varsayılanları değil).
    public List<string> Styles { get; set; } = new();

    public int MinTemp { get; set; }
    public int MaxTemp { get; set; }
    public bool AvoidWhenWet { get; set; }
    public bool PreferredWhenWet { get; set; }
    public bool PreferredWhenHot { get; set; }

    /// Dolapta yalnızca "onayli"; geçmişte sonradan silinmiş parça "silindi".
    public string Status { get; set; } = string.Empty;
}

/// Geçmişteki bir karar (§6, history kuralları).

public class RecommendationHistoryEntry
{
    public long ComparisonId { get; set; }

    /// Yalnızca KT, KD, KR, OT, OD, OR, HB. Diğerlerini Python reddeder.
    public string ResultKey { get; set; } = string.Empty;

    /// comparisons.completed_at. DateTimeOffset, çünkü Python saat dilimi olmayan tarihi reddeder.
    public DateTimeOffset CompletedAt { get; set; }

    /// Karar anındaki bağlam (comparison_sessions'tan).
    public RecommendationHistoryContext Context { get; set; } = new();

    public RecommendationOutfit RuleOutfit { get; set; } = new();
    public RecommendationOutfit LearnedOutfit { get; set; } = new();

    /// Yalnızca KD ve OD'de, kombinin son hâli kaydedildiyse dolu; yoksa null gider.
    public RecommendationOutfit? FinalOutfit { get; set; }
}

/// Geçmiş kararın kısa bağlamı.

public class RecommendationHistoryContext
{
    public decimal FeelsLikeTemp { get; set; }
    public string WeatherCondition { get; set; } = string.Empty;
    public string EventStyle { get; set; } = string.Empty;
}

/// Geçmişteki bir kombin: şablon ve parçalar.

public class RecommendationOutfit
{
    /// Kombinde tek parça varsa "tek_parca", yoksa "ust_alt". C# hesaplar.
    public string Template { get; set; } = string.Empty;

    public List<RecommendationItem> Items { get; set; } = new();
}

/// /recommend cevabı (C#'ın kullandığı hâli). JSON'a çevrilmez; Python'un cevabını istemci okur ve buna doldurur.

public class OutfitRecommendationResult
{
    /// false ise Python "no_candidates" dedi; aşağıdaki listeler boştur.
    public bool HasCandidates { get; set; }

    /// Kural Kartının ana parçaları (dış giyim dahil) → comparisons.rule_outfit.
    public List<long> RuleItemIds { get; set; } = new();

    /// Öğrenen Kartın ana parçaları → comparisons.learned_outfit.
    public List<long> LearnedItemIds { get; set; } = new();

    /// true ise iki yöntem aynı kombini seçti; uygulamaya tek kart gider (isSingleCard).
    public bool IsSameOutfit { get; set; }

    /// soguk_baslangic, kesif ya da model → comparisons.learned_pick_mode_id. Uygulamaya GÖNDERİLMEZ.
    public string LearnedPickMode { get; set; } = string.Empty;

    /// İki karta da eklenecek aksesuarlar → comparisons.accessories. Aksesuar istenmediyse boş.
    public List<long> AccessoryIds { get; set; } = new();
}