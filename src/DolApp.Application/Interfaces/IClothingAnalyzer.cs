namespace DolApp.Application.Interfaces;

using DolApp.Application.Models;

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
