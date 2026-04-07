using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace PSImageSharp;

/// <summary>
/// Wraps a SixLabors.ImageSharp Image together with its source path and detected format,
/// providing a pipeline-friendly object that displays useful information in the PowerShell console.
/// </summary>
public sealed class PSImage : IDisposable
{
    private bool _disposed;

    /// <summary>Gets the underlying ImageSharp Image.</summary>
    public Image Image { get; }

    /// <summary>Gets the file path that was used to load this image (empty string if created in-memory).</summary>
    public string SourcePath { get; }

    /// <summary>Gets the detected image format (e.g. "jpeg", "png", "webp").</summary>
    public string Format { get; }

    /// <summary>Gets the image width in pixels.</summary>
    public int Width => Image.Width;

    /// <summary>Gets the image height in pixels.</summary>
    public int Height => Image.Height;

    public PSImage(Image image, string sourcePath, IImageFormat format)
    {
        Image = image;
        SourcePath = sourcePath;
        Format = format?.Name ?? "unknown";
    }

    public override string ToString() =>
        $"PSImage: {Width}x{Height} [{Format}]{(string.IsNullOrEmpty(SourcePath) ? "" : $" ({SourcePath})")}";

    public void Dispose()
    {
        if (!_disposed)
        {
            Image?.Dispose();
            _disposed = true;
        }
    }
}
