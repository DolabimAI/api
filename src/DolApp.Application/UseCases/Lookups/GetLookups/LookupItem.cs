namespace DolApp.Application.UseCases.Lookups.GetLookups;

/// Bir liste öğesi: { "id": 2, "value": "gomlek", "displayName": "Gömlek" }.
/// value kod içinde (sabit, değişmez), displayName ekranda kullanılır.

public class LookupItem
{
    public long Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}