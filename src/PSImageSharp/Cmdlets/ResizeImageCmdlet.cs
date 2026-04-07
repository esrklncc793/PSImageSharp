using System.Management.Automation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PSImageSharp.Cmdlets;

/// <summary>
/// <para type="synopsis">Resizes a PSImage to the specified dimensions.</para>
/// <para type="description">
/// Resize-Image scales the image using high-quality Lanczos3 resampling.
///
/// Dimension rules:
///   - Supply -Width and -Height to scale to exact pixel dimensions (ignores aspect ratio).
///   - Supply only -Width or only -Height to scale proportionally, preserving aspect ratio.
///   - Supply -Percentage (e.g. 50) to scale both axes by that percentage.
///
/// The mutated PSImage is passed through the pipeline.
/// </para>
/// <example>
///   <code>Open-Image "photo.jpg" | Resize-Image -Width 800 | Save-Image "thumb.jpg"</code>
/// </example>
/// <example>
///   <code>Open-Image "photo.jpg" | Resize-Image -Width 1920 -Height 1080 | Save-Image "wallpaper.jpg"</code>
/// </example>
/// <example>
///   <code>Open-Image "photo.jpg" | Resize-Image -Percentage 50 | Save-Image "half.jpg"</code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommon.Resize, "Image", DefaultParameterSetName = ByDimensions)]
[OutputType(typeof(PSImage))]
public sealed class ResizeImageCmdlet : PSCmdlet
{
    private const string ByDimensions = "ByDimensions";
    private const string ByPercentage = "ByPercentage";

    /// <summary>
    /// <para type="description">The PSImage object to resize.</para>
    /// </summary>
    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public PSImage? InputObject { get; set; }

    /// <summary>
    /// <para type="description">Target width in pixels. If only -Width is specified, height is calculated to maintain the aspect ratio.</para>
    /// </summary>
    [Parameter(ParameterSetName = ByDimensions)]
    [ValidateRange(1, int.MaxValue)]
    public int Width { get; set; }

    /// <summary>
    /// <para type="description">Target height in pixels. If only -Height is specified, width is calculated to maintain the aspect ratio.</para>
    /// </summary>
    [Parameter(ParameterSetName = ByDimensions)]
    [ValidateRange(1, int.MaxValue)]
    public int Height { get; set; }

    /// <summary>
    /// <para type="description">Scale factor as a percentage (e.g. 50 for half size, 200 for double size).</para>
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = ByPercentage)]
    [ValidateRange(1, 10000)]
    public double Percentage { get; set; }

    protected override void ProcessRecord()
    {
        if (InputObject is null)
            return;

        int srcWidth  = InputObject.Width;
        int srcHeight = InputObject.Height;

        int targetWidth;
        int targetHeight;

        if (ParameterSetName == ByPercentage)
        {
            double scale = Percentage / 100.0;
            targetWidth  = Math.Max(1, (int)Math.Round(srcWidth  * scale));
            targetHeight = Math.Max(1, (int)Math.Round(srcHeight * scale));
        }
        else
        {
            bool hasWidth  = MyInvocation.BoundParameters.ContainsKey(nameof(Width));
            bool hasHeight = MyInvocation.BoundParameters.ContainsKey(nameof(Height));

            if (!hasWidth && !hasHeight)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("At least one of -Width or -Height must be supplied."),
                    "ResizeMissingDimension",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            if (hasWidth && hasHeight)
            {
                // Both supplied: exact resize (ignores aspect ratio).
                targetWidth  = Width;
                targetHeight = Height;
            }
            else if (hasWidth)
            {
                // Only width: preserve aspect ratio.
                targetWidth  = Width;
                targetHeight = Math.Max(1, (int)Math.Round((double)srcHeight / srcWidth * Width));
            }
            else
            {
                // Only height: preserve aspect ratio.
                targetHeight = Height;
                targetWidth  = Math.Max(1, (int)Math.Round((double)srcWidth / srcHeight * Height));
            }
        }

        WriteVerbose($"Resizing {srcWidth}x{srcHeight} -> {targetWidth}x{targetHeight}");

        InputObject.Image.Mutate(ctx =>
            ctx.Resize(new ResizeOptions
            {
                Size    = new Size(targetWidth, targetHeight),
                Mode    = ResizeMode.Stretch,
                Sampler = KnownResamplers.Lanczos3,
            }));

        WriteObject(InputObject);
    }
}
