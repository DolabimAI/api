using System.Net;
using DolApp.Application.Interfaces;
using DolApp.Core.Errors;
using DolApp.Infrastructure.Clients;
using DolApp.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace DolApp.Tests.Clients;

/// Python /analyze istemcisinin sözleşmedeki hata eşleştirmesi (contracts/python-api.md, 5. bölüm).
/// Gerçek Python'a gidilmez: cevaplar StubHttpMessageHandler'dan gelir.

public class PythonClothingAnalyzerTests
{
    private static readonly Guid TestUserId = Guid.Parse("178eaf32-7cf1-4399-8957-6532e4c7fc91");

    /// contracts/examples/analyze-response.json ile aynı.
    private const string ValidAnalyzeResponse = """
        {
          "imagePath": "178eaf32-7cf1-4399-8957-6532e4c7fc91/0b7e3c1a9f4d4e2c.webp",
          "subtype": "gomlek",
          "subtypeConfidence": 0.873,
          "color": "beyaz",
          "tone": "acik",
          "isPatterned": false,
          "rawColor": "#F2F0EB"
        }
        """;

    private readonly StubHttpMessageHandler _stubHttpMessageHandler = new();

    [Fact]
    public async Task AnalyzeAsync_WhenPythonReturns200_MapsAllFields()
    {
        _stubHttpMessageHandler.ResponseBody = ValidAnalyzeResponse;

        var clothingAnalysis = await AnalyzeAsync();

        Assert.Equal("178eaf32-7cf1-4399-8957-6532e4c7fc91/0b7e3c1a9f4d4e2c.webp", clothingAnalysis.ImagePath);
        Assert.Equal("gomlek", clothingAnalysis.SubtypeValue);
        Assert.Equal(0.873m, clothingAnalysis.SubtypeConfidence);
        Assert.Equal("beyaz", clothingAnalysis.ColorValue);
        Assert.Equal("acik", clothingAnalysis.ToneValue);
        Assert.False(clothingAnalysis.IsPatterned);
        Assert.Equal("#F2F0EB", clothingAnalysis.RawColor);
    }

    [Fact]
    public async Task AnalyzeAsync_SendsImageAndUserIdFieldsToAnalyzeEndpoint()
    {
        _stubHttpMessageHandler.ResponseBody = ValidAnalyzeResponse;

        await AnalyzeAsync();

        // Alan adları sözleşmeyle aynı olmalı: "image" ve "userId".
        Assert.Equal("/analyze", _stubHttpMessageHandler.LastRequest!.RequestUri!.AbsolutePath);
        // .NET adı tırnaklı ya da tırnaksız yazabilir: name=image ya da name="image".
        Assert.Matches("name=\"?image\"?;", _stubHttpMessageHandler.LastRequestBody);
        Assert.Matches("name=\"?userId\"?", _stubHttpMessageHandler.LastRequestBody);
        Assert.Contains(TestUserId.ToString(), _stubHttpMessageHandler.LastRequestBody);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenPythonReturns422ImageUnreadable_Throws400ImageUnreadable()
    {
        _stubHttpMessageHandler.ResponseStatusCode = HttpStatusCode.UnprocessableEntity;
        _stubHttpMessageHandler.ResponseBody = """{ "code": "image_unreadable", "message": "Fotoğraf okunamadı." }""";

        var appException = await Assert.ThrowsAsync<AppException>(AnalyzeAsync);

        Assert.Equal(400, appException.StatusCode);
        Assert.Equal(ErrorCodes.ImageUnreadable, appException.Code);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenPythonReturns422InvalidRequest_Throws502()
    {
        // 422'nin her türü image_unreadable değil: C#'ın isteği bozuksa bu bizim hatamız, kullanıcıya 502.
        _stubHttpMessageHandler.ResponseStatusCode = HttpStatusCode.UnprocessableEntity;
        _stubHttpMessageHandler.ResponseBody = """{ "code": "invalid_request", "message": "..." }""";

        var appException = await Assert.ThrowsAsync<AppException>(AnalyzeAsync);

        Assert.Equal(502, appException.StatusCode);
        Assert.Equal(ErrorCodes.AnalysisFailed, appException.Code);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenPythonReturns500_Throws502()
    {
        _stubHttpMessageHandler.ResponseStatusCode = HttpStatusCode.InternalServerError;
        _stubHttpMessageHandler.ResponseBody = """{ "code": "internal_error", "message": "..." }""";

        var appException = await Assert.ThrowsAsync<AppException>(AnalyzeAsync);

        Assert.Equal(502, appException.StatusCode);
        Assert.Equal(ErrorCodes.AnalysisFailed, appException.Code);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenPythonIsDown_Throws502()
    {
        // T-14'ün C# kısmı: servis kapalı (bağlantı reddedildi).
        _stubHttpMessageHandler.ExceptionToThrow = new HttpRequestException("Connection refused");

        var appException = await Assert.ThrowsAsync<AppException>(AnalyzeAsync);

        Assert.Equal(502, appException.StatusCode);
        Assert.Equal(ErrorCodes.AnalysisFailed, appException.Code);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenTimeoutExpires_Throws502()
    {
        // T-14'ün C# kısmı: 30 sn aşıldı. HttpClient zaman aşımını TaskCanceledException olarak fırlatır.
        _stubHttpMessageHandler.ExceptionToThrow = new TaskCanceledException("Timeout");

        var appException = await Assert.ThrowsAsync<AppException>(AnalyzeAsync);

        Assert.Equal(502, appException.StatusCode);
        Assert.Equal(ErrorCodes.AnalysisFailed, appException.Code);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenResponseMissesAField_Throws502()
    {
        _stubHttpMessageHandler.ResponseBody = """{ "imagePath": "a/b.webp", "subtype": "gomlek" }""";

        var appException = await Assert.ThrowsAsync<AppException>(AnalyzeAsync);

        Assert.Equal(502, appException.StatusCode);
        Assert.Equal(ErrorCodes.AnalysisFailed, appException.Code);
    }

    private async Task<ClothingAnalysis> AnalyzeAsync()
    {
        var httpClient = new HttpClient(_stubHttpMessageHandler) { BaseAddress = new Uri("http://python.test/") };
        var pythonClothingAnalyzer = new PythonClothingAnalyzer(
            httpClient,
            new PythonOptions { BaseUrl = "http://python.test" },
            NullLogger<PythonClothingAnalyzer>.Instance);

        using var imageStream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        return await pythonClothingAnalyzer.AnalyzeAsync(imageStream, "foto.jpg", "image/jpeg", TestUserId, CancellationToken.None);
    }
}