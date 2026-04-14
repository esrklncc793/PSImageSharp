using System.Management.Automation;
using PSImageSharp.Models;
using PSImageSharp.Services;

namespace PSImageSharp.Cmdlets;

/// <summary>
/// <para type="synopsis">Applies an image watermark to one or more images.</para>
/// <para type="description">
/// The Add-ImageWatermark cmdlet overlays a watermark image onto a base image.
/// The watermark is scaled relative to the shorter side of the base image and
/// placed at the specified position with the given opacity.
/// </para>
/// </summary>
[Cmdlet(VerbsCommon.Add, "ImageWatermark", SupportsShouldProcess = true)]
[OutputType(typeof(System.IO.FileInfo))]
public sealed class AddImageWatermarkCommand : PSCmdlet
{
    /// <summary>
    /// <para type="description">Path to the base image file to watermark.</para>
    /// </summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [Alias("FullName")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// <para type="description">Path to the watermark image file.</para>
    /// </summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string WatermarkPath { get; set; } = string.Empty;

    /// <summary>
    /// <para type="description">
    /// Output file path. Defaults to overwriting the source image.
    /// </para>
    /// </summary>
    [Parameter(Position = 2)]
    public string? Destination { get; set; }

    /// <summary>
    /// <para type="description">
    /// Watermark opacity between 0.0 (fully transparent) and 1.0 (fully opaque). Default is 0.5.
    /// </para>
    /// </summary>
    [Parameter]
    [ValidateRange(0.0f, 1.0f)]
    public float Opacity { get; set; } = 0.5f;

    /// <summary>
    /// <para type="description">
    /// Position of the watermark on the base image. Default is BottomRight.
    /// </para>
    /// </summary>
    [Parameter]
    public WatermarkPosition Position { get; set; } = WatermarkPosition.BottomRight;

    /// <summary>
    /// <para type="description">
    /// Watermark size as a fraction of the shorter side of the base image (0.01–1.0). Default is 0.2.
    /// </para>
    /// </summary>
    [Parameter]
    [ValidateRange(0.01f, 1.0f)]
    public float Scale { get; set; } = 0.2f;

    /// <summary>
    /// <para type="description">When specified, returns a FileInfo object for the output file.</para>
    /// </summary>
    [Parameter]
    public SwitchParameter PassThru { get; set; }

    protected override void ProcessRecord()
    {
        var sourcePath = GetUnresolvedProviderPathFromPSPath(Path);
        var watermarkSourcePath = GetUnresolvedProviderPathFromPSPath(WatermarkPath);
        var outputPath = string.IsNullOrEmpty(Destination)
            ? sourcePath
            : GetUnresolvedProviderPathFromPSPath(Destination);

        if (!ShouldProcess(sourcePath, "Add image watermark"))
        {
            return;
        }

        try
        {
            ImageWatermarkService.ApplyWatermark(sourcePath, watermarkSourcePath, outputPath, Opacity, Position, Scale);
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(ex, "WatermarkFailed", ErrorCategory.WriteError, sourcePath));
            return;
        }

        if (PassThru)
        {
            WriteObject(new System.IO.FileInfo(outputPath));
        }
    }
}
