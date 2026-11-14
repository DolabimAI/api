using DolApp.Core.Entities;

namespace DolApp.Application.Interfaces;

/// cities ve districts tablolarına erişim. Şehrim kontrolü (C2) ve il/ilçe/en yakın ilçe uçları (C4) kullanır.

public interface ILocationRepository
{
    Task<bool> CityExistsAsync(long cityId, CancellationToken cancellationToken);

    /// İlçe var ve verilen ile aitse true döner.
    Task<bool> DistrictBelongsToCityAsync(long districtId, long cityId, CancellationToken cancellationToken);

    Task<List<City>> GetCitiesAsync(CancellationToken cancellationToken);

    Task<List<District>> GetDistrictsByCityIdAsync(long cityId, CancellationToken cancellationToken);

    /// Koordinata en yakın ilçeyi ili ile birlikte getirir.
    Task<District?> GetNearestDistrictAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken);

    Task<City?> GetCityByIdAsync(long cityId, CancellationToken cancellationToken);

    Task<District?> GetDistrictByIdAsync(long districtId, CancellationToken cancellationToken);
}