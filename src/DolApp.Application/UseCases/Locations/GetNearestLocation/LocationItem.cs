namespace DolApp.Application.UseCases.Locations.GetNearestLocation;

/// En yakın konumdaki il ya da ilçe: { "id": 34, "name": "İstanbul" }.

public class LocationItem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}