namespace DolApp.Application.UseCases.Locations.GetCities;

/// GET /api/cities cevabı. Controller yalnızca Cities listesini döner (API belgesindeki biçim düz dizidir).

public class GetCitiesResponse
{
    public List<CityItem> Cities { get; set; } = new();
}