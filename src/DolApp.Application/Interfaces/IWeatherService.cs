using DolApp.Core.Entities;

namespace DolApp.Application.Interfaces;

/// Hava özeti: GET /api/weather (C4) ve kombin önerisi (C5) aynı hesabı kullanır.

public interface IWeatherService
{
    /// Konum, gün ve saat aralığı (başlangıç ve bitiş dahil) için ortalama hissedilen sıcaklık ve en ağır hava durumu.
    /// Hava alınamazsa 502 weather_unavailable fırlatır.
    Task<WeatherSummary> GetWeatherSummaryAsync(
        decimal latitude, decimal longitude, DateOnly date, int startHour, int endHour,
        CancellationToken cancellationToken);
}

/// Hava özeti. Condition, weather_conditions tablosundaki satırın kendisidir:
/// C5 ıslak/güneşli bilgisini (IsWet, IsSunny) ve id'sini buradan alır.

public class WeatherSummary
{
    public decimal FeelsLikeTemperature { get; set; }
    public WeatherCondition Condition { get; set; } = null!;
}