namespace DolApp.Application.Interfaces;

/// Supabase depo (bucket: gorseller). Depo herkese açık değil: uygulamaya görsel yerine süreli bağlantı gider.
/// Görseli depoya Python yazar; C# yalnızca bağlantı üretir ve siler.

public interface IImageStorage
{
    /// Tek görsel için süreli bağlantı. Görsel depoda yoksa null döner ve loga uyarı yazar (cevap yine döner).
    Task<string?> CreateSignedUrlAsync(string imagePath, CancellationToken cancellationToken);

    /// Birden fazla görsel için tek istekte süreli bağlantı (Dolabım listesi). Bulunamayan görsel sözlükte yer almaz.
    Task<IReadOnlyDictionary<string, string>> CreateSignedUrlsAsync(
        IReadOnlyCollection<string> imagePaths, CancellationToken cancellationToken);

    /// Görseli siler. Görsel zaten yoksa hata vermez (tekrar çağrılabilir).
    Task DeleteAsync(string imagePath, CancellationToken cancellationToken);
}