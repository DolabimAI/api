namespace DolApp.Application.Interfaces;

/// cities ve districts tablolarına erişim. Şehrim kontrolü (C2) ve il/ilçe/en yakın ilçe uçları (C4) kullanır.

public interface ILocationRepository
{
    Task<bool> CityExistsAsync(long cityId, CancellationToken cancellationToken);

    /// İlçe var ve verilen ile aitse true döner.
    Task<bool> DistrictBelongsToCityAsync(long districtId, long cityId, CancellationToken cancellationToken);
}