namespace DolApp.Application.UseCases.Clothes.GetClothes;

/// Dolabım'daki bir alt tür grubu. Grupların listesi uygulamadaki filtre seçeneklerini de verir.

public class ClothingGroup
{
    public long SubtypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<ClothingListItem> Items { get; set; } = new();
}