namespace DolApp.Application.UseCases.Wardrobe.GetWardrobeStatus;

/// Şablonun bir kategorisi: { "category": "ust", "have": 4, "need": 6 }.
/// category kodla gelir; uygulama ekrandaki adı lookups'taki kategorilerden bulur.

public class WardrobeRequirementItem
{
    public string Category { get; set; } = string.Empty;
    public int Have { get; set; }
    public int Need { get; set; }
}