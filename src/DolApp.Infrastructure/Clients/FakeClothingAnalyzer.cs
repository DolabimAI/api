using DolApp.Application.Interfaces;
using DolApp.Core.Errors;
using Microsoft.Extensions.Logging;
using DolApp.Application.Models;

namespace DolApp.Infrastructure.Clients;

/// Python /analyze'ın taklidi. YALNIZCA geliştirme içindir: appsettings.Development.json'da Python:UseFake = true.
/// D7 birleşince ayar false yapılır ve bu sınıf kullanılmaz (PR'da kalır; testte işe yarar).
///
/// Davranış:
///   - Cevap, sözleşmedeki örnekle aynı (contracts/examples/analyze-response.json): gomlek, beyaz, acik, desensiz.
///   - Güven Python:FakeConfidence'tan gelir; 0,40'ın altına çekilirse "tür boş" durumu denenir.
///   - Gerçek akışta görseli Python yazar. Süreli bağlantı ve silme de denenebilsin diye taklit,
///     depoya küçük bir yer tutucu PNG yükler: <userId>/fake-<rastgele>.png
///   - Dosya adında "okunamaz" geçerse Python'un 422'si taklit edilir: 400 image_unreadable, hiçbir şey yüklenmez.

public class FakeClothingAnalyzer : IClothingAnalyzer
{
    /// 64×64, açık bej (#E5E0D8) PNG; 137 bayt.
    private static readonly byte[] PlaceholderPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAIAAAAlC+aJAAAAUElEQVR42u3PMQ0AAAgDsPlXyMmDDyRwkzSpgWa6XouAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAwGUBHqrSaIGkxRQAAAAASUVORK5CYII=");

    private readonly SupabaseImageStorage _supabaseImageStorage;
    private readonly PythonOptions _pythonOptions;
    private readonly ILogger<FakeClothingAnalyzer> _logger;

    public FakeClothingAnalyzer(
        SupabaseImageStorage supabaseImageStorage, PythonOptions pythonOptions, ILogger<FakeClothingAnalyzer> logger)
    {
        _supabaseImageStorage = supabaseImageStorage;
        _pythonOptions = pythonOptions;
        _logger = logger;
    }

    public async Task<ClothingAnalysis> AnalyzeAsync(
        Stream imageStream, string fileName, string contentType, Guid userId, CancellationToken cancellationToken)
    {
        if (fileName.Contains("okunamaz", StringComparison.OrdinalIgnoreCase))
            throw new AppException(400, ErrorCodes.ImageUnreadable, ErrorMessages.ImageUnreadable);

        var imagePath = $"{userId}/fake-{Guid.NewGuid():N}.png";
        await _supabaseImageStorage.UploadPlaceholderAsync(imagePath, PlaceholderPng, cancellationToken);
        _logger.LogInformation("Python taklidi: {FileName} için yer tutucu görsel yüklendi ({ImagePath}).", fileName, imagePath);

        return new ClothingAnalysis
        {
            ImagePath = imagePath,
            SubtypeValue = "gomlek",
            SubtypeConfidence = _pythonOptions.FakeConfidence,
            ColorValue = "beyaz",
            ToneValue = "acik",
            IsPatterned = false,
            RawColor = "#F2F0EB",
        };
    }

    public Task CheckHealthAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Python taklit modunda (Python:UseFake = true): /health kontrolü atlandı, /analyze örnek cevap dönüyor.");
        return Task.CompletedTask;
    }
}