using MediatR;

namespace DolApp.Application.UseCases.Locations.GetNearestLocation;

/// GET /api/locations/nearest?lat=..&lon=.. isteği. Değerler adresteki sorgu parametrelerinden gelir.
/// null: parametre gönderilmemiş (validator required_fields verir). Konum hiçbir yere kaydedilmez.

public class GetNearestLocationRequest : IRequest<GetNearestLocationResponse>
{
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}