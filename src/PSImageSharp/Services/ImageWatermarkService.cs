using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using PSImageSharp.Models;

namespace PSImageSharp.Services;

/// <summary>
/// Provides image watermarking functionality using SixLabors.ImageSharp.
/// </summary>
public static class ImageWatermarkService
{
    private const int DefaultMargin = 5;

    /// <summary>
    /// Applies an image watermark to a base image and saves the result.
    /// </summary>
    /// <param name="sourcePath">Path to the base image.</param>
    /// <param name="watermarkPath">Path to the watermark image.</param>
    /// <param name="outputPath">Path where the output image is saved.</param>
    /// <param name="opacity">Opacity of the watermark (0.0 transparent – 1.0 opaque).</param>
    /// <param name="position">Corner or centre position for the watermark.</param>
    /// <param name="scale">
    /// Watermark size as a fraction of the shorter side of the base image (e.g. 0.2 = 20 %).
    /// </param>
    public static void ApplyWatermark(
        string sourcePath,
        string watermarkPath,
        string outputPath,
        float opacity = 0.5f,
        WatermarkPosition position = WatermarkPosition.BottomRight,
        float scale = 0.2f)
    {
        using var baseImage = Image.Load<Rgba32>(sourcePath);
        using var watermarkImage = Image.Load<Rgba32>(watermarkPath);

        ApplyWatermark(baseImage, watermarkImage, opacity, position, scale);

        baseImage.Save(outputPath);
    }

    /// <summary>
    /// Applies an image watermark directly to an in-memory <see cref="Image{Rgba32}"/>.
    /// </summary>
    public static void ApplyWatermark(
        Image<Rgba32> baseImage,
        Image<Rgba32> watermarkImage,
        float opacity = 0.5f,
        WatermarkPosition position = WatermarkPosition.BottomRight,
        float scale = 0.2f)
    {
        // Resize watermark proportionally relative to the shorter side of the base image.
        int shorterSide = Math.Min(baseImage.Width, baseImage.Height);
        int watermarkWidth = (int)(shorterSide * scale);
        int watermarkHeight = (int)((double)watermarkImage.Height / watermarkImage.Width * watermarkWidth);

        // Clone so the caller's watermark image is not mutated.
        using var scaledWatermark = watermarkImage.Clone(ctx => ctx.Resize(watermarkWidth, watermarkHeight));

        var location = CalculatePosition(baseImage.Size, scaledWatermark.Size, position);

        baseImage.Mutate(ctx => ctx.DrawImage(scaledWatermark, location, opacity));
    }

    public static Point CalculatePosition(Size baseSize, Size watermarkSize, WatermarkPosition position)
    {
        int right = baseSize.Width - watermarkSize.Width - DefaultMargin;
        int bottom = baseSize.Height - watermarkSize.Height - DefaultMargin;

        return position switch
        {
            WatermarkPosition.TopLeft => new Point(DefaultMargin, DefaultMargin),
            WatermarkPosition.TopRight => new Point(right, DefaultMargin),
            WatermarkPosition.BottomLeft => new Point(DefaultMargin, bottom),
            WatermarkPosition.BottomRight => new Point(right, bottom),
            WatermarkPosition.Center => new Point(
                (baseSize.Width - watermarkSize.Width) / 2,
                (baseSize.Height - watermarkSize.Height) / 2),
            _ => new Point(right, bottom)
        };
    }
}
