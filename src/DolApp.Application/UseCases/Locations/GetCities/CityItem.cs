namespace DolApp.Application.UseCases.Locations.GetCities;

/// İl listesindeki bir il: { "id": 34, "name": "İstanbul", "plateCode": 34 }. Koordinat dönmez.

public class CityItem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PlateCode { get; set; }
}