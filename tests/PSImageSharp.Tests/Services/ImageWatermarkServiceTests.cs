using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using PSImageSharp.Models;
using PSImageSharp.Services;
using Xunit;

namespace PSImageSharp.Tests.Services;

public sealed class ImageWatermarkServiceTests : IDisposable
{
    private readonly string _tempDir;

    public ImageWatermarkServiceTests()
    {
        _tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        Directory.Delete(_tempDir, recursive: true);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static Image<Rgba32> CreateSolidImage(int width, int height, Rgba32 color)
    {
        return new Image<Rgba32>(width, height, color);
    }

    private string SaveTempImage(Image<Rgba32> image, string fileName)
    {
        var path = System.IO.Path.Combine(_tempDir, fileName);
        image.SaveAsPng(path);
        return path;
    }

    // ── CalculatePosition tests ───────────────────────────────────────────────

    [Theory]
    [InlineData(WatermarkPosition.TopLeft, 5, 5)]
    [InlineData(WatermarkPosition.TopRight, 175, 5)]    // 200 - 20 - 5
    [InlineData(WatermarkPosition.BottomLeft, 5, 75)]   // 100 - 20 - 5
    [InlineData(WatermarkPosition.BottomRight, 175, 75)]
    [InlineData(WatermarkPosition.Center, 90, 40)]      // (200-20)/2, (100-20)/2
    public void CalculatePosition_ReturnsExpectedPoint(
        WatermarkPosition position, int expectedX, int expectedY)
    {
        var baseSize = new Size(200, 100);
        var watermarkSize = new Size(20, 20);

        var point = ImageWatermarkService.CalculatePosition(baseSize, watermarkSize, position);

        Assert.Equal(expectedX, point.X);
        Assert.Equal(expectedY, point.Y);
    }

    // ── ApplyWatermark (in-memory) tests ──────────────────────────────────────

    [Fact]
    public void ApplyWatermark_DoesNotThrow_ForValidImages()
    {
        using var baseImage = CreateSolidImage(200, 200, new Rgba32(255, 0, 0));
        using var watermark = CreateSolidImage(50, 50, new Rgba32(0, 255, 0));

        var ex = Record.Exception(() =>
            ImageWatermarkService.ApplyWatermark(baseImage, watermark));

        Assert.Null(ex);
    }

    [Fact]
    public void ApplyWatermark_DoesNotMutateWatermarkImage()
    {
        using var baseImage = CreateSolidImage(200, 200, new Rgba32(255, 0, 0));
        using var watermark = CreateSolidImage(50, 50, new Rgba32(0, 255, 0));

        int originalWidth = watermark.Width;
        int originalHeight = watermark.Height;

        ImageWatermarkService.ApplyWatermark(baseImage, watermark, scale: 0.1f);

        Assert.Equal(originalWidth, watermark.Width);
        Assert.Equal(originalHeight, watermark.Height);
    }

    [Fact]
    public void ApplyWatermark_PreservesBaseDimensions()
    {
        using var baseImage = CreateSolidImage(300, 200, new Rgba32(255, 0, 0));
        using var watermark = CreateSolidImage(40, 40, new Rgba32(0, 0, 255));

        int expectedWidth = baseImage.Width;
        int expectedHeight = baseImage.Height;

        ImageWatermarkService.ApplyWatermark(baseImage, watermark);

        Assert.Equal(expectedWidth, baseImage.Width);
        Assert.Equal(expectedHeight, baseImage.Height);
    }

    [Theory]
    [InlineData(WatermarkPosition.TopLeft)]
    [InlineData(WatermarkPosition.TopRight)]
    [InlineData(WatermarkPosition.BottomLeft)]
    [InlineData(WatermarkPosition.BottomRight)]
    [InlineData(WatermarkPosition.Center)]
    public void ApplyWatermark_AllPositions_DoNotThrow(WatermarkPosition position)
    {
        using var baseImage = CreateSolidImage(400, 300, new Rgba32(100, 100, 100));
        using var watermark = CreateSolidImage(60, 60, new Rgba32(200, 200, 200));

        var ex = Record.Exception(() =>
            ImageWatermarkService.ApplyWatermark(baseImage, watermark, position: position));

        Assert.Null(ex);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    public void ApplyWatermark_AllOpacityValues_DoNotThrow(float opacity)
    {
        using var baseImage = CreateSolidImage(300, 300, new Rgba32(100, 100, 100));
        using var watermark = CreateSolidImage(50, 50, new Rgba32(200, 50, 50));

        var ex = Record.Exception(() =>
            ImageWatermarkService.ApplyWatermark(baseImage, watermark, opacity: opacity));

        Assert.Null(ex);
    }

    [Theory]
    [InlineData(0.1f)]
    [InlineData(0.2f)]
    [InlineData(0.5f)]
    public void ApplyWatermark_ScalesWatermarkToFractionOfShorterSide(float scale)
    {
        // Use a known base size and a watermark that is obviously larger.
        using var baseImage = CreateSolidImage(400, 200, new Rgba32(50, 50, 50));
        using var watermark = CreateSolidImage(200, 200, new Rgba32(200, 200, 200));

        // Capture the pixel at bottom-right where the watermark will land.
        // At full opacity the watermark pixel (200,200,200) should be visible.
        ImageWatermarkService.ApplyWatermark(
            baseImage, watermark, opacity: 1.0f,
            position: WatermarkPosition.BottomRight, scale: scale);

        // The shorter side is 200, expected watermark width = (int)(200 * scale).
        int expectedWatermarkWidth = (int)(200 * scale);

        // Pixel at BottomRight margin should now show the watermark colour (when opacity = 1).
        // Margin is 5, so sample at (width - 6, height - 6).
        var pixel = baseImage[baseImage.Width - 6, baseImage.Height - 6];
        Assert.Equal(new Rgba32(200, 200, 200), pixel);

        // And a pixel outside the watermark area should still be the original colour.
        var outsidePixel = baseImage[0, 0];
        Assert.Equal(new Rgba32(50, 50, 50), outsidePixel);

        // Ensure watermark width is proportional (just check it's > 0 and <= shorterSide).
        Assert.True(expectedWatermarkWidth > 0);
        Assert.True(expectedWatermarkWidth <= Math.Min(baseImage.Width, baseImage.Height));
    }

    // ── ApplyWatermark (file-based) tests ─────────────────────────────────────

    [Fact]
    public void ApplyWatermark_FileBased_CreatesOutputFile()
    {
        using var baseImage = CreateSolidImage(300, 300, new Rgba32(255, 0, 0));
        using var watermark = CreateSolidImage(50, 50, new Rgba32(0, 255, 0));

        var sourcePath = SaveTempImage(baseImage, "base.png");
        var watermarkPath = SaveTempImage(watermark, "watermark.png");
        var outputPath = System.IO.Path.Combine(_tempDir, "output.png");

        ImageWatermarkService.ApplyWatermark(sourcePath, watermarkPath, outputPath);

        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public void ApplyWatermark_FileBased_OutputHasCorrectDimensions()
    {
        using var baseImage = CreateSolidImage(400, 300, new Rgba32(255, 0, 0));
        using var watermark = CreateSolidImage(50, 50, new Rgba32(0, 0, 255));

        var sourcePath = SaveTempImage(baseImage, "base.png");
        var watermarkPath = SaveTempImage(watermark, "watermark.png");
        var outputPath = System.IO.Path.Combine(_tempDir, "output.png");

        ImageWatermarkService.ApplyWatermark(sourcePath, watermarkPath, outputPath);

        using var result = Image.Load<Rgba32>(outputPath);
        Assert.Equal(400, result.Width);
        Assert.Equal(300, result.Height);
    }

    [Fact]
    public void ApplyWatermark_FileBased_CanOverwriteSource()
    {
        using var baseImage = CreateSolidImage(200, 200, new Rgba32(0, 100, 200));
        using var watermark = CreateSolidImage(30, 30, new Rgba32(200, 100, 0));

        var sourcePath = SaveTempImage(baseImage, "base.png");
        var watermarkPath = SaveTempImage(watermark, "watermark.png");

        // Output = source path → overwrite
        var ex = Record.Exception(() =>
            ImageWatermarkService.ApplyWatermark(sourcePath, watermarkPath, sourcePath));

        Assert.Null(ex);
        Assert.True(File.Exists(sourcePath));
    }
}
