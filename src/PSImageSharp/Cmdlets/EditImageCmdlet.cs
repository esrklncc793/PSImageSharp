using System.Management.Automation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PSImageSharp.Cmdlets;

/// <summary>
/// <para type="synopsis">Applies one or more quick filters / adjustments to a PSImage.</para>
/// <para type="description">
/// Edit-Image is a Swiss-army knife cmdlet that exposes the most commonly needed image
/// adjustments as individual switch / value parameters.  Multiple adjustments can be
/// combined in a single call; they are applied in the order listed below.
///
/// Supported adjustments:
///   -Grayscale            Convert to greyscale.
///   -Brightness &lt;float&gt;   Adjust brightness. Range: -1.0 to 1.0 (0 = no change).
///   -Contrast   &lt;float&gt;   Adjust contrast.   Range: -1.0 to 1.0 (0 = no change).
///   -Saturation &lt;float&gt;   Adjust saturation. Range: -1.0 to 1.0 (0 = no change).
///   -Hue        &lt;float&gt;   Rotate hue by degrees. Range: -360 to 360 (0 = no change).
///   -FlipHorizontal       Flip image horizontally.
///   -FlipVertical         Flip image vertically.
///   -RotateDegrees &lt;int&gt;  Rotate clockwise by this many degrees (90, 180, 270 are lossless).
///   -AutoOrient           Apply EXIF orientation tag, then strip it.
/// </para>
/// <example>
///   <code>Open-Image "photo.jpg" | Edit-Image -Grayscale | Save-Image "grey.jpg"</code>
/// </example>
/// <example>
///   <code>Open-Image "photo.jpg" | Edit-Image -Brightness 0.1 -Contrast 0.1 | Save-Image "vivid.jpg"</code>
/// </example>
/// </summary>
[Cmdlet(VerbsData.Edit, "Image")]
[OutputType(typeof(PSImage))]
public sealed class EditImageCmdlet : PSCmdlet
{
    /// <summary><para type="description">The PSImage object to edit.</para></summary>
    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public PSImage? InputObject { get; set; }

    /// <summary><para type="description">Convert the image to greyscale.</para></summary>
    [Parameter]
    public SwitchParameter Grayscale { get; set; }

    /// <summary><para type="description">Brightness adjustment in the range -1.0 to 1.0. 0 = no change.</para></summary>
    [Parameter]
    [ValidateRange(-1.0f, 1.0f)]
    public float Brightness { get; set; }

    /// <summary><para type="description">Contrast adjustment in the range -1.0 to 1.0. 0 = no change.</para></summary>
    [Parameter]
    [ValidateRange(-1.0f, 1.0f)]
    public float Contrast { get; set; }

    /// <summary><para type="description">Saturation adjustment in the range -1.0 to 1.0. 0 = no change.</para></summary>
    [Parameter]
    [ValidateRange(-1.0f, 1.0f)]
    public float Saturation { get; set; }

    /// <summary><para type="description">Hue rotation in degrees (-360 to 360). 0 = no change.</para></summary>
    [Parameter]
    [ValidateRange(-360f, 360f)]
    public float Hue { get; set; }

    /// <summary><para type="description">Flip the image horizontally (mirror).</para></summary>
    [Parameter]
    public SwitchParameter FlipHorizontal { get; set; }

    /// <summary><para type="description">Flip the image vertically.</para></summary>
    [Parameter]
    public SwitchParameter FlipVertical { get; set; }

    /// <summary><para type="description">Rotate the image clockwise by this many degrees.</para></summary>
    [Parameter]
    [ValidateRange(-360, 360)]
    public int RotateDegrees { get; set; }

    /// <summary><para type="description">Apply the EXIF orientation tag and strip it (auto-rotate).</para></summary>
    [Parameter]
    public SwitchParameter AutoOrient { get; set; }

    protected override void ProcessRecord()
    {
        if (InputObject is null)
            return;

        InputObject.Image.Mutate(ctx =>
        {
            if (AutoOrient.IsPresent)
                ctx.AutoOrient();

            if (Grayscale.IsPresent)
                ctx.Grayscale();

            if (MyInvocation.BoundParameters.ContainsKey(nameof(Brightness)))
                ctx.Brightness(1.0f + Brightness);

            if (MyInvocation.BoundParameters.ContainsKey(nameof(Contrast)))
                ctx.Contrast(1.0f + Contrast);

            if (MyInvocation.BoundParameters.ContainsKey(nameof(Saturation)))
                ctx.Saturate(1.0f + Saturation);

            if (MyInvocation.BoundParameters.ContainsKey(nameof(Hue)))
                ctx.Hue(Hue);

            if (FlipHorizontal.IsPresent)
                ctx.Flip(FlipMode.Horizontal);

            if (FlipVertical.IsPresent)
                ctx.Flip(FlipMode.Vertical);

            if (MyInvocation.BoundParameters.ContainsKey(nameof(RotateDegrees)) && RotateDegrees != 0)
                ctx.Rotate(RotateDegrees);
        });

        WriteObject(InputObject);
    }
}
