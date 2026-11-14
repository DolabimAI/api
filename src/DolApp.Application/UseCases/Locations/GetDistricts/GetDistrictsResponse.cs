namespace DolApp.Application.UseCases.Locations.GetDistricts;

/// GET /api/cities/{id}/districts cevabı. Controller yalnızca Districts listesini döner.

public class GetDistrictsResponse
{
    public List<DistrictItem> Districts { get; set; } = new();
}