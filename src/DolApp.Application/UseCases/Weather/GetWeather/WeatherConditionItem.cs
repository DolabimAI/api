namespace DolApp.Application.UseCases.Weather.GetWeather;

/// Hava durumu: { "value": "yagmurlu", "displayName": "Yağmurlu" }.

public class WeatherConditionItem
{
    public string Value { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}