using System.Management.Automation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace PSImageSharp.Cmdlets;

/// <summary>
/// <para type="synopsis">Saves a PSImage object to disk in the specified format.</para>
/// <para type="description">
/// Save-Image encodes the PSImage object received from the pipeline and writes it to the
/// supplied path.  The output format is inferred from the file extension (.jpg/.jpeg, .png,
/// .webp); you can also override it with the -Format parameter.
/// An optional -Quality parameter (1-100) controls the JPEG / WebP lossy quality.
/// The original PSImage object is passed through to the pipeline so that the chain can
/// continue.
/// </para>
/// <example>
///   <code>Open-Image "photo.jpg" | Resize-Image -Width 800 | Save-Image -Path "thumb.jpg"</code>
/// </example>
/// </summary>
[Cmdlet(VerbsData.Save, "Image")]
[OutputType(typeof(PSImage))]
public sealed class SaveImageCmdlet : PSCmdlet
{
    /// <summary>
    /// <para type="description">The PSImage object to save.</para>
    /// </summary>
    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public PSImage? InputObject { get; set; }

    /// <summary>
    /// <para type="description">Destination file path. The extension determines the output format unless -Format is specified.</para>
    /// </summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// <para type="description">Override the output format: Jpeg, Png, or Webp.</para>
    /// </summary>
    [Parameter]
    [ValidateSet("Jpeg", "Png", "Webp", IgnoreCase = true)]
    public string? Format { get; set; }

    /// <summary>
    /// <para type="description">Quality for lossy formats (JPEG, WebP). Range 1-100. Default: 90.</para>
    /// </summary>
    [Parameter]
    [ValidateRange(1, 100)]
    public int Quality { get; set; } = 90;

    /// <summary>
    /// <para type="description">Pass the PSImage object through to the pipeline after saving.</para>
    /// </summary>
    [Parameter]
    public SwitchParameter PassThru { get; set; }

    protected override void ProcessRecord()
    {
        if (InputObject is null)
            return;

        string resolvedPath = GetUnresolvedProviderPathFromPSPath(Path);

        string? directory = System.IO.Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            ThrowTerminatingError(new ErrorRecord(
                new DirectoryNotFoundException($"Output directory does not exist: {directory}"),
                "OutputDirectoryNotFound",
                ErrorCategory.ObjectNotFound,
                resolvedPath));
            return;
        }

        IImageEncoder encoder = ResolveEncoder(resolvedPath);

        try
        {
            InputObject.Image.Save(resolvedPath, encoder);
            WriteVerbose($"Saved image to: {resolvedPath}");

            if (PassThru.IsPresent)
                WriteObject(InputObject);
        }
        catch (Exception ex) when (ex is not PipelineStoppedException)
        {
            ThrowTerminatingError(new ErrorRecord(
                ex,
                "ImageSaveError",
                ErrorCategory.WriteError,
                resolvedPath));
        }
    }

    private IImageEncoder ResolveEncoder(string resolvedPath)
    {
        string fmt = Format ?? System.IO.Path.GetExtension(resolvedPath).TrimStart('.').ToLowerInvariant();

        return fmt switch
        {
            "jpeg" or "jpg" => new JpegEncoder { Quality = Quality },
            "webp"          => new WebpEncoder  { Quality = Quality },
            "png"           => new PngEncoder(),
            _               => new JpegEncoder  { Quality = Quality },
        };
    }
}
