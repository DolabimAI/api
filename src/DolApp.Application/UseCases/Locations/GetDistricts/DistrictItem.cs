namespace DolApp.Application.UseCases.Locations.GetDistricts;

/// İlçe listesindeki bir ilçe: { "id": 442, "name": "Kadıköy" }. Koordinat dönmez.

public class DistrictItem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}