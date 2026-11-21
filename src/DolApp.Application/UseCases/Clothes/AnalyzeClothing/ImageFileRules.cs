using DolApp.Core.Errors;

namespace DolApp.Application.UseCases.Clothes.AnalyzeClothing;

/// Yüklenen fotoğrafın Python'a gitmeden önceki kontrolü: boyut ve tür.
/// Tür, dosya adına ya da telefonun gönderdiği Content-Type'a değil, dosyanın ilk baytlarına bakılarak bulunur
/// (adı .jpg olan bir metin dosyası da reddedilir). Kabul edilenler: JPEG, PNG, WebP. HEIC şimdilik kabul edilmez.

public static class ImageFileRules
{
    /// 10 MB. Telefon fotoğrafları çoğunlukla 2–6 MB; Python küçültüp 1 MB altına indiriyor.
    public const long MaxFileBytes = 10 * 1024 * 1024;

    /// Türü anlamak için okunan bayt sayısı (WebP'nin imzası 12. bayta kadar uzanıyor).
    private const int HeaderLength = 12;

    /// Boyutu ve türü kontrol eder; geçerse Python'a gönderilecek türü döner (image/jpeg, image/png ya da image/webp).
    /// Boyut aşılırsa 413 file_too_large, tür desteklenmiyorsa (boş dosya dahil) 400 unsupported_file fırlatır.
    /// Akış sonunda başa sarılır; Python'a dosyanın tamamı gider.
    public static async Task<string> ValidateAsync(Stream imageStream, long fileLength, CancellationToken cancellationToken)
    {
        if (fileLength > MaxFileBytes)
            throw new AppException(413, ErrorCodes.FileTooLarge, ErrorMessages.FileTooLarge);

        var header = await ReadHeaderAsync(imageStream, cancellationToken);
        return DetectContentType(header)
            ?? throw new AppException(400, ErrorCodes.UnsupportedFile, ErrorMessages.UnsupportedFile);
    }

    /// İlk baytlara (dosya imzası) bakarak türü bulur. Tanınmazsa null.
    ///   JPEG: FF D8 FF
    ///   PNG:  89 50 4E 47 0D 0A 1A 0A
    ///   WebP: "RIFF" + 4 bayt boyut + "WEBP"
    public static string? DetectContentType(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return "image/jpeg";

        if (header.Length >= 8 && header[..8].SequenceEqual(PngSignature))
            return "image/png";

        if (header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8))
            return "image/webp";

        return null;
    }

    private static ReadOnlySpan<byte> PngSignature => new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    /// İlk 12 baytı okur ve akışı başa sarar. Yüklenen dosyanın akışı başa sarılabilir (ASP.NET dosyayı tamponlar).
    private static async Task<byte[]> ReadHeaderAsync(Stream imageStream, CancellationToken cancellationToken)
    {
        if (!imageStream.CanSeek)
            throw new InvalidOperationException("Fotoğraf akışı başa sarılamıyor; tür kontrolünden sonra Python'a eksik gider.");

        var buffer = new byte[HeaderLength];
        var readCount = await imageStream.ReadAtLeastAsync(buffer, HeaderLength, throwOnEndOfStream: false, cancellationToken);
        imageStream.Seek(0, SeekOrigin.Begin);
        return buffer[..readCount];
    }
}