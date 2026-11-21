namespace DolApp.Application.Interfaces;

/// Fotoğrafı Python'un /analyze ucuna gönderir
/// Python temiz görseli depoya yazar ve tahmini döner; veritabanına C# yazar.
/// Gerçek istemci PythonClothingAnalyzer; D7 gelene kadar ayarla FakeClothingAnalyzer kullanılabilir (Python:UseFake).

public interface IClothingAnalyzer
{
    /// Fotoğraf okunamazsa 400 image_unreadable, Python hata verir ya da süre aşılırsa 502 analysis_failed fırlatır.
    Task<ClothingAnalysis> AnalyzeAsync(
        Stream imageStream, string fileName, string contentType, Guid userId, CancellationToken cancellationToken);

    /// Python'un /health ucunu çağırır ve sonucu loga yazar. Hata fırlatmaz (D1'in "C# /health'e ulaşıyor" kriteri).
    Task CheckHealthAsync(CancellationToken cancellationToken);
}

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