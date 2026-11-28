namespace DolApp.Application.Models;

/// Python'un /analyze cevabı. Liste değerleri kodla gelir (gomlek, beyaz, acik); C# bunları id'ye çevirir.

public class ClothingAnalysis
{
    /// Görselin bucket içindeki yolu: <userId>/<rastgele-ad>.webp
    public string ImagePath { get; set; } = string.Empty;

    /// En olası alt tür (güven ne olursa olsun). 0,40 eşiğini C# uygular.
    public string SubtypeValue { get; set; } = string.Empty;

    /// 0 ile 1 arası, 3 ondalık.
    public decimal SubtypeConfidence { get; set; }

    public string ColorValue { get; set; } = string.Empty;
    public string ToneValue { get; set; } = string.Empty;
    public bool IsPatterned { get; set; }

    /// Ölçülen ham renk, #RRGGBB.
    public string RawColor { get; set; } = string.Empty;
}