namespace DolApp.Application.UseCases.Comparisons.Common;

/// Uygulamaya giden karşılaştırma kartı: sessions (C5) ve reject-both (C6) cevaplarında aynı biçim.
/// Yöntem bilgisi (kural / öğrenen) bilerek yoktur; katılımcı hangi kartın hangi yöntemden geldiğini bilmemeli.

public class ComparisonCard
{
    public long Id { get; set; }
    public int SequenceNo { get; set; }

    /// true ise iki yöntem aynı kombini seçti: kombin Left'te gelir, Right boştur. Uygulama select çağırmaz.
    public bool IsSingleCard { get; set; }

    public List<OutfitCardItem> Left { get; set; } = new();
    public List<OutfitCardItem> Right { get; set; } = new();

    /// İki karta da eklenen aksesuarlar (aksesuar istenmediyse boş).
    public List<AccessoryCardItem> Accessories { get; set; } = new();
}