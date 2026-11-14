using System.Text.Json.Serialization;

namespace DolApp.Application.UseCases.Weather.GetWeather;

/// GET /api/weather cevabı: { "feelsLikeTemp": 9.4, "condition": {...}, "isDaytime": false }.

public class GetWeatherResponse
{
    /// Vaktin saatlerindeki ortalama hissedilen sıcaklık (°C, 1 ondalık).
    /// JSON adı API belgesindeki gibi "feelsLikeTemp"; kodda kısaltmasız ad kullanılır.
    [JsonPropertyName("feelsLikeTemp")]
    public decimal FeelsLikeTemperature { get; set; }

    /// Vaktin saatlerindeki en ağır hava durumu.
    public WeatherConditionItem Condition { get; set; } = new();

    /// Vakit gündüz mü (sabah, öğle) gece mi (akşam, gece)? times_of_day.is_daytime'dan gelir.
    public bool IsDaytime { get; set; }
}