using DolApp.Application.Interfaces;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Weather.GetWeather;

/// Seçilen yer, gün ve vaktin hava özeti. Sıra: vakit var mı → bugün için geçti mi → koordinat (ilçe, yoksa il)
/// → hava servisi. Hava hesabının kendisi IWeatherService'tedir (C5 de kullanır).

public class GetWeatherHandler : IRequestHandler<GetWeatherRequest, GetWeatherResponse>
{
    private readonly ILookupRepository _lookupRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IWeatherService _weatherService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetWeatherHandler(
        ILookupRepository lookupRepository,
        ILocationRepository locationRepository,
        IWeatherService weatherService,
        IDateTimeProvider dateTimeProvider)
    {
        _lookupRepository = lookupRepository;
        _locationRepository = locationRepository;
        _weatherService = weatherService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<GetWeatherResponse> Handle(GetWeatherRequest request, CancellationToken cancellationToken)
    {
        // Validator zorunlu alanların dolu olduğunu garanti eder.
        var cityId = request.CityId!.Value;
        var date = request.Date!.Value;
        var timeOfDayId = request.TimeOfDayId!.Value;

        var timesOfDay = await _lookupRepository.GetTimesOfDayAsync(cancellationToken);
        var timeOfDay = timesOfDay.FirstOrDefault(timeOfDayItem => timeOfDayItem.Id == timeOfDayId)
            ?? throw new AppException(400, ErrorCodes.InvalidId, ErrorMessages.InvalidId);

        // Bugün için bitiş saati geçmiş vakit seçilemez (ör. saat 21:00'de "Akşam 17–20").
        // Vakit sürüyorsa seçilebilir; ortalama yine vaktin bütün saatleri üzerinden alınır.
        var isToday = date == _dateTimeProvider.TodayInIstanbul;
        if (isToday && _dateTimeProvider.NowInIstanbul.Hour > timeOfDay.EndHour)
            throw new AppException(400, ErrorCodes.TimePassed, ErrorMessages.TimePassed);

        var (latitude, longitude) = await GetCoordinatesAsync(cityId, request.DistrictId, cancellationToken);

        var weatherSummary = await _weatherService.GetWeatherSummaryAsync(
            latitude, longitude, date, timeOfDay.StartHour, timeOfDay.EndHour, cancellationToken);

        return new GetWeatherResponse
        {
            FeelsLikeTemperature = weatherSummary.FeelsLikeTemperature,
            Condition = new WeatherConditionItem
            {
                Value = weatherSummary.Condition.Value,
                DisplayName = weatherSummary.Condition.DisplayName,
            },
            IsDaytime = timeOfDay.IsDaytime,
        };
    }

    /// İlçe verilmişse ilçenin (o ile ait olmalı), verilmemişse ilin koordinatı.
    private async Task<(decimal Latitude, decimal Longitude)> GetCoordinatesAsync(
        long cityId, long? districtId, CancellationToken cancellationToken)
    {
        if (districtId is not null)
        {
            var district = await _locationRepository.GetDistrictByIdAsync(districtId.Value, cancellationToken);
            if (district is null || district.CityId != cityId)
                throw new AppException(400, ErrorCodes.DistrictNotInCity, ErrorMessages.DistrictNotInCity);

            return (district.Latitude, district.Longitude);
        }

        var city = await _locationRepository.GetCityByIdAsync(cityId, cancellationToken)
            ?? throw new AppException(400, ErrorCodes.InvalidId, ErrorMessages.InvalidId);

        return (city.Latitude, city.Longitude);
    }
}