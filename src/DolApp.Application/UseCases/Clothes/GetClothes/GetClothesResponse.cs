namespace DolApp.Application.UseCases.Clothes.GetClothes;

/// GET /api/clothes cevabı (200). Örnek (API belgesi):
/// { "totalCount": 32, "groups": [ { "subtypeId": 26, "name": "Bot", "count": 2, "items": [ { "id": 1290, "imageUrl": "https://..." } ] } ] }
/// totalCount: dolaptaki bütün onaylı parçalar, filtreden bağımsız (Ana sayfadaki sayıyla aynı).
/// groups: yalnızca parçası olan türler, adına göre Türkçe alfabetik; filtre verildiyse yalnızca o tür.

public class GetClothesResponse
{
    public int TotalCount { get; set; }
    public List<ClothingGroup> Groups { get; set; } = new();
}