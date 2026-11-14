namespace DolApp.Application.Interfaces;

/// Uygulamanın saati. "Şimdi" ve "bugün" her zaman Europe/Istanbul'a göre hesaplanır;
/// bilgisayarın saat ayarına güvenilmez (plan kuralı). Testte sahte bir saat verilebilir.

public interface IDateTimeProvider
{
    /// Veritabanına yazılacak zaman damgaları için (timestamptz UTC ister).
    DateTime UtcNow { get; }

    /// İstanbul'da şu anki saat (ör. bugün için geçmiş vakit kontrolü).
    DateTime NowInIstanbul { get; }

    /// İstanbul'da bugünün tarihi (ör. 18 yaş kontrolü, +6 gün kontrolü).
    DateOnly TodayInIstanbul { get; }
}
