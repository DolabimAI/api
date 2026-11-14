using MediatR;

namespace DolApp.Application.UseCases.Weather.GetWeather;

/// GET /api/weather?cityId=..&districtId=..&date=..&timeOfDayId=.. isteği. Değerler sorgu parametrelerinden gelir.
/// districtId isteğe bağlı: verilmezse ilin koordinatı kullanılır. Diğerleri zorunlu (null = gönderilmemiş).

public class GetWeatherRequest : IRequest<GetWeatherResponse>
{
    public long? CityId { get; set; }
    public long? DistrictId { get; set; }
    public DateOnly? Date { get; set; }
    public long? TimeOfDayId { get; set; }
}