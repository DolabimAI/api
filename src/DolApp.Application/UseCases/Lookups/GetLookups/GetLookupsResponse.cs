namespace DolApp.Application.UseCases.Lookups.GetLookups;

/// GET /api/lookups cevabı: uygulamanın bütün açılır listeleri.
/// methods ve result_statuses bilerek yok (yöntem gizliliği).

public class GetLookupsResponse
{
    public List<LookupItem> Categories { get; set; } = new();
    public List<SubtypeLookupItem> Subtypes { get; set; } = new();
    public List<LookupItem> Colors { get; set; } = new();
    public List<LookupItem> Tones { get; set; } = new();
    public List<LookupItem> Styles { get; set; } = new();
    public List<LookupItem> Seasons { get; set; } = new();
    public List<LookupItem> Thicknesses { get; set; } = new();
    public List<TimeOfDayLookupItem> TimesOfDay { get; set; } = new();
}