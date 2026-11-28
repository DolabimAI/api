namespace DolApp.Application.UseCases.Comparisons.Common;

/// Karttaki bir ana parça. Görsel bağlantısı 1 saat geçerli; görseli olmayan parçada null.

public class OutfitCardItem
{
    public long Id { get; set; }
    public long CategoryId { get; set; }
    public string? ImageUrl { get; set; }
}