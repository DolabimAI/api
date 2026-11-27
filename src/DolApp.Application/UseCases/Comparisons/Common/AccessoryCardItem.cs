namespace DolApp.Application.UseCases.Comparisons.Common;

/// Karttaki bir aksesuar. Görsel bağlantısı 1 saat geçerli; görseli olmayan parçada null.

public class AccessoryCardItem
{
    public long Id { get; set; }
    public string? ImageUrl { get; set; }
}