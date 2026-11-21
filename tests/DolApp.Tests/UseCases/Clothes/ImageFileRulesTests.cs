using System.Text;
using DolApp.Application.UseCases.Clothes.AnalyzeClothing;
using DolApp.Core.Errors;

namespace DolApp.Tests.UseCases.Clothes;

/// Fotoğraf kontrolü: tür dosyanın ilk baytlarından bulunuyor mu, boyut sınırı çalışıyor mu.
/// Gerçek fotoğraf gerekmez: her türün yalnızca imza baytları kullanılır.

public class ImageFileRulesTests
{
    private static readonly byte[] JpegHeader = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 };
    private static readonly byte[] PngHeader = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D };

    /// "RIFF" + 4 bayt boyut + "WEBP"
    private static readonly byte[] WebpHeader = { 0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 };

    /// iPhone'un HEIC dosyası: 4. bayttan sonra "ftypheic".
    private static readonly byte[] HeicHeader = { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x68, 0x65, 0x69, 0x63 };

    [Fact]
    public async Task ValidateAsync_WhenJpeg_ReturnsImageJpeg()
    {
        Assert.Equal("image/jpeg", await ValidateAsync(JpegHeader));
    }

    [Fact]
    public async Task ValidateAsync_WhenPng_ReturnsImagePng()
    {
        Assert.Equal("image/png", await ValidateAsync(PngHeader));
    }

    [Fact]
    public async Task ValidateAsync_WhenWebp_ReturnsImageWebp()
    {
        Assert.Equal("image/webp", await ValidateAsync(WebpHeader));
    }

    [Fact]
    public async Task ValidateAsync_WhenHeic_Throws400UnsupportedFile()
    {
        var appException = await Assert.ThrowsAsync<AppException>(() => ValidateAsync(HeicHeader));

        Assert.Equal(400, appException.StatusCode);
        Assert.Equal(ErrorCodes.UnsupportedFile, appException.Code);
    }

    [Fact]
    public async Task ValidateAsync_WhenTextFileNamedJpg_Throws400UnsupportedFile()
    {
        // Adı ya da Content-Type'ı ne olursa olsun, içerik metinse reddedilir.
        var appException = await Assert.ThrowsAsync<AppException>(() => ValidateAsync(Encoding.UTF8.GetBytes("bu bir fotoğraf değil")));

        Assert.Equal(ErrorCodes.UnsupportedFile, appException.Code);
    }

    [Fact]
    public async Task ValidateAsync_WhenEmptyFile_Throws400UnsupportedFile()
    {
        var appException = await Assert.ThrowsAsync<AppException>(() => ValidateAsync(Array.Empty<byte>()));

        Assert.Equal(ErrorCodes.UnsupportedFile, appException.Code);
    }

    [Fact]
    public async Task ValidateAsync_WhenLargerThan10Mb_Throws413FileTooLarge()
    {
        // Boyut, akış okunmadan bildirilen uzunluktan kontrol edilir; 10 MB'lık dizi oluşturmaya gerek yok.
        var appException = await Assert.ThrowsAsync<AppException>(
            () => ValidateAsync(JpegHeader, fileLength: ImageFileRules.MaxFileBytes + 1));

        Assert.Equal(413, appException.StatusCode);
        Assert.Equal(ErrorCodes.FileTooLarge, appException.Code);
    }

    [Fact]
    public async Task ValidateAsync_WhenExactly10Mb_IsAccepted()
    {
        Assert.Equal("image/jpeg", await ValidateAsync(JpegHeader, fileLength: ImageFileRules.MaxFileBytes));
    }

    [Fact]
    public async Task ValidateAsync_RewindsStreamToStart()
    {
        // Python'a dosyanın tamamı gitmeli: kontrol akışı okuduktan sonra başa sarmalı.
        using var imageStream = new MemoryStream(JpegHeader);

        await ImageFileRules.ValidateAsync(imageStream, imageStream.Length, CancellationToken.None);

        Assert.Equal(0, imageStream.Position);
    }

    private static async Task<string> ValidateAsync(byte[] fileBytes, long? fileLength = null)
    {
        using var imageStream = new MemoryStream(fileBytes);
        return await ImageFileRules.ValidateAsync(imageStream, fileLength ?? fileBytes.Length, CancellationToken.None);
    }
}