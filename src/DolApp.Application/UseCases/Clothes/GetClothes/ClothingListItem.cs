namespace DolApp.Application.UseCases.Clothes.GetClothes;

/// Listedeki tek parça: yalnızca id ve görsel. Ayrıntı için GET /api/clothes/{id}.

public class ClothingListItem
{
    public long Id { get; set; }

    /// Süreli görsel bağlantısı (1 saat). Görsel depoda bulunamazsa null.
    public string? ImageUrl { get; set; }
}