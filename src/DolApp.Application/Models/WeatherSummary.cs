using DolApp.Core.Entities;

namespace DolApp.Application.Models;

/// Hava özeti. Condition, weather_conditions tablosundaki satırın kendisidir:
/// C5 ıslak/güneşli bilgisini (IsWet, IsSunny) ve id'sini buradan alır.

public class WeatherSummary
{
    public decimal FeelsLikeTemperature { get; set; }
    public WeatherCondition Condition { get; set; } = null!;
}