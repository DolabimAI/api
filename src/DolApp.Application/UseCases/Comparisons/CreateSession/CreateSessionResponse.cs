using DolApp.Application.UseCases.Comparisons.Common;
using DolApp.Application.UseCases.Weather.GetWeather;

namespace DolApp.Application.UseCases.Comparisons.CreateSession;

/// POST /api/comparisons/sessions cevabı (201). weather, GET /api/weather cevabıyla aynı biçim.

public class CreateSessionResponse
{
    public long SessionId { get; set; }
    public GetWeatherResponse Weather { get; set; } = new();
    public ComparisonCard Comparison { get; set; } = new();
}