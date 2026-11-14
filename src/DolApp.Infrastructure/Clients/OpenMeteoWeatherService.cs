using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DolApp.Application.Interfaces;
using DolApp.Core.Entities;
using DolApp.Core.Errors;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DolApp.Infrastructure.Clients;

/// Open-Meteo'dan saatlik tahmin alır ve hava özetini hesaplar (tek hava sınıfı; C4 ve C5 kullanır).
/// Bir konumun bir günlük tahmini 30 dakika bellekte tutulur: aynı gün için vakit değiştirmek yeni istek atmaz.
/// Open-Meteo'ya kullanım koşulu gereği uygulamada atıf verilir (Sude, ana sayfa).

public class OpenMeteoWeatherService : IWeatherService
{
    private const string IstanbulTimeZone = "Europe/Istanbul";
    private static readonly TimeSpan ForecastCacheDuration = TimeSpan.FromMinutes(30);

    private readonly HttpClient _httpClient;
    private readonly OpenMeteoOptions _openMeteoOptions;
    private readonly ILookupRepository _lookupRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<OpenMeteoWeatherService> _logger;

    public OpenMeteoWeatherService(
        HttpClient httpClient,
        OpenMeteoOptions openMeteoOptions,
        ILookupRepository lookupRepository,
        IMemoryCache memoryCache,
        ILogger<OpenMeteoWeatherService> logger)
    {
        _httpClient = httpClient;
        _openMeteoOptions = openMeteoOptions;
        _lookupRepository = lookupRepository;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<WeatherSummary> GetWeatherSummaryAsync(
        decimal latitude, decimal longitude, DateOnly date, int startHour, int endHour,
        CancellationToken cancellationToken)
    {
        var hourlyForecasts = await GetHourlyForecastsAsync(latitude, longitude, date, cancellationToken);

        // Vaktin saatleri (bitiş dahil).
        var hoursInTimeRange = hourlyForecasts
            .Where(hourlyForecast => hourlyForecast.Hour >= startHour && hourlyForecast.Hour <= endHour)
            .ToList();
        if (hoursInTimeRange.Count == 0)
            throw CreateWeatherUnavailableException($"{date:yyyy-MM-dd} {startHour}-{endHour} saatleri için veri yok.");

        // Ortalama hissedilen sıcaklık, 1 ondalık (ekranda uygulama tam sayıya yuvarlar).
        var averageFeelsLikeTemperature = Math.Round(
            hoursInTimeRange.Average(hourlyForecast => hourlyForecast.ApparentTemperature),
            1, MidpointRounding.AwayFromZero);

        // En ağır hava durumu: saatlerin hava kodları tablodaki türlere çevrilir, severity'si en yüksek olan seçilir.
        var weatherConditions = await _lookupRepository.GetWeatherConditionsAsync(cancellationToken);
        var worstWeatherCondition = hoursInTimeRange
            .Select(hourlyForecast => FindWeatherCondition(weatherConditions, hourlyForecast.WeatherCode))
            .Where(weatherCondition => weatherCondition is not null)
            .OrderByDescending(weatherCondition => weatherCondition!.Severity)
            .FirstOrDefault();
        if (worstWeatherCondition is null)
            throw CreateWeatherUnavailableException("Saatlerin hiçbirinin hava kodu tabloda yok.");

        return new WeatherSummary
        {
            FeelsLikeTemperature = averageFeelsLikeTemperature,
            Condition = worstWeatherCondition,
        };
    }

    /// Bir konumun bir günlük (24 saat) tahmini: önce bellekte aranır, yoksa Open-Meteo'dan alınır.
    private async Task<List<HourlyForecast>> GetHourlyForecastsAsync(
        decimal latitude, decimal longitude, DateOnly date, CancellationToken cancellationToken)
    {
        // Sayılar her zaman noktalı yazılır: Türkçe ortamda C# virgül kullanır ("40,98"), Open-Meteo nokta bekler.
        var latitudeText = latitude.ToString(CultureInfo.InvariantCulture);
        var longitudeText = longitude.ToString(CultureInfo.InvariantCulture);
        var dateText = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var cacheKey = $"weather:{latitudeText}:{longitudeText}:{dateText}";
        if (_memoryCache.TryGetValue(cacheKey, out List<HourlyForecast>? cachedHourlyForecasts) && cachedHourlyForecasts is not null)
            return cachedHourlyForecasts;

        var requestUri = $"{_openMeteoOptions.BaseUrl}?latitude={latitudeText}&longitude={longitudeText}"
            + "&hourly=apparent_temperature,weather_code"
            + $"&timezone={Uri.EscapeDataString(IstanbulTimeZone)}"
            + $"&start_date={dateText}&end_date={dateText}";

        OpenMeteoForecastResponse? forecastResponse;
        try
        {
            forecastResponse = await _httpClient.GetFromJsonAsync<OpenMeteoForecastResponse>(requestUri, cancellationToken);
        }
        catch (Exception exception) when (
            (exception is HttpRequestException or TaskCanceledException or JsonException)
            && !cancellationToken.IsCancellationRequested)
        {
            // Bağlantı hatası, zaman aşımı ya da bozuk cevap. Kullanıcı isteği iptal ettiyse buraya girilmez.
            throw CreateWeatherUnavailableException($"Open-Meteo isteği başarısız: {requestUri}", exception);
        }

        var hourlyData = forecastResponse?.Hourly;
        if (hourlyData?.Time is null || hourlyData.ApparentTemperature is null || hourlyData.WeatherCode is null)
            throw CreateWeatherUnavailableException($"Open-Meteo cevabında saatlik veri yok: {requestUri}");

        var hourlyForecasts = new List<HourlyForecast>();
        for (var index = 0; index < hourlyData.Time.Count; index++)
        {
            var apparentTemperature = index < hourlyData.ApparentTemperature.Count ? hourlyData.ApparentTemperature[index] : null;
            var weatherCode = index < hourlyData.WeatherCode.Count ? hourlyData.WeatherCode[index] : null;

            // Değeri eksik saat hesaba katılmaz.
            if (apparentTemperature is null || weatherCode is null)
                continue;

            // Saat "2026-11-25T17:00" biçiminde, İstanbul saatiyle gelir.
            var hour = DateTime.ParseExact(hourlyData.Time[index], "yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture).Hour;
            hourlyForecasts.Add(new HourlyForecast(hour, apparentTemperature.Value, weatherCode.Value));
        }

        _memoryCache.Set(cacheKey, hourlyForecasts, ForecastCacheDuration);
        return hourlyForecasts;
    }

    /// WMO hava kodunu weather_conditions'taki türe çevirir. Tabloda olmayan kod loga yazılır ve hesaba katılmaz.
    private WeatherCondition? FindWeatherCondition(List<WeatherCondition> weatherConditions, int weatherCode)
    {
        var weatherCondition = weatherConditions.FirstOrDefault(condition => condition.ApiCodes.Contains(weatherCode));
        if (weatherCondition is null)
            _logger.LogWarning("Hava kodu {WeatherCode} weather_conditions tablosunda yok.", weatherCode);

        return weatherCondition;
    }

    /// Ayrıntıyı loga yazar; kullanıcıya genel 502 weather_unavailable döner.
    private AppException CreateWeatherUnavailableException(string reason, Exception? innerException = null)
    {
        _logger.LogWarning(innerException, "Hava durumu alınamadı: {Reason}", reason);
        return new AppException(502, ErrorCodes.WeatherUnavailable, ErrorMessages.WeatherUnavailable);
    }

    // ---- Open-Meteo JSON cevabından yalnızca ihtiyacımız olan alanlar ----

    private sealed record OpenMeteoForecastResponse(
        [property: JsonPropertyName("hourly")] OpenMeteoHourlyData? Hourly);

    private sealed record OpenMeteoHourlyData(
        [property: JsonPropertyName("time")] List<string>? Time,
        [property: JsonPropertyName("apparent_temperature")] List<decimal?>? ApparentTemperature,
        [property: JsonPropertyName("weather_code")] List<int?>? WeatherCode);

    private sealed record HourlyForecast(int Hour, decimal ApparentTemperature, int WeatherCode);
}