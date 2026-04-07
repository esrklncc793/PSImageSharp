using System.Management.Automation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace PSImageSharp.Cmdlets;

/// <summary>
/// <para type="synopsis">Loads an image from disk and outputs a PSImage object to the pipeline.</para>
/// <para type="description">
/// Open-Image reads an image file from the specified path and outputs a PSImage wrapper object.
/// The object carries the underlying SixLabors.ImageSharp Image, the source path, and the
/// detected format, and can be passed directly to Resize-Image, Edit-Image, or Save-Image.
/// </para>
/// <example>
///   <code>Open-Image -Path "photo.jpg"</code>
/// </example>
/// <example>
///   <code>Open-Image -Path "photo.jpg" | Resize-Image -Width 800 | Save-Image -Path "thumb.jpg"</code>
/// </example>
/// </summary>
[Cmdlet(VerbsCommon.Open, "Image")]
[OutputType(typeof(PSImage))]
public sealed class OpenImageCmdlet : PSCmdlet
{
    /// <summary>
    /// <para type="description">The file-system path to the image to open.</para>
    /// </summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [Alias("FullName")]
    public string Path { get; set; } = string.Empty;

    protected override void ProcessRecord()
    {
        string resolvedPath = GetUnresolvedProviderPathFromPSPath(Path);

        if (!File.Exists(resolvedPath))
        {
            ThrowTerminatingError(new ErrorRecord(
                new FileNotFoundException($"Image file not found: {resolvedPath}"),
                "ImageFileNotFound",
                ErrorCategory.ObjectNotFound,
                resolvedPath));
            return;
        }

        try
        {
            IImageFormat format = Image.DetectFormat(resolvedPath)
                ?? throw new NotSupportedException($"Unable to detect the image format for: {resolvedPath}");
            Image image = Image.Load(resolvedPath);
            WriteObject(new PSImage(image, resolvedPath, format));
        }
        catch (Exception ex) when (ex is not PipelineStoppedException)
        {
            ThrowTerminatingError(new ErrorRecord(
                ex,
                "ImageLoadError",
                ErrorCategory.ReadError,
                resolvedPath));
        }
    }
}
