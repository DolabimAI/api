namespace DolApp.Application.UseCases.Locations.GetNearestLocation;

/// GET /api/locations/nearest cevabı: en yakın ilçe ve ili.

public class GetNearestLocationResponse
{
    public LocationItem City { get; set; } = new();
    public LocationItem District { get; set; } = new();
}