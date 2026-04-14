# PSImageSharp

A PowerShell binary module for image manipulation built on [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp).

## Requirements

- PowerShell 7.0+
- .NET 8 SDK (for building)

## Building

```powershell
./Build.ps1
```

The compiled module is placed in `output/PSImageSharp/`.

```powershell
Import-Module ./output/PSImageSharp
```

Pass `-Clean` to remove the output directory before building, or `-Configuration Debug` to build a debug version.

## Cmdlets

### `Add-ImageWatermark`

Overlays a watermark image onto a base image.

#### Parameters

| Parameter       | Type              | Default        | Description |
|-----------------|-------------------|----------------|-------------|
| `-Path`         | `string`          | *(required)*   | Path to the base image. Accepts pipeline input / `FullName` property. |
| `-WatermarkPath`| `string`          | *(required)*   | Path to the watermark image. |
| `-Destination`  | `string`          | *(overwrites source)* | Output file path. |
| `-Opacity`      | `float` 0.0–1.0   | `0.5`          | Watermark opacity. |
| `-Position`     | `WatermarkPosition` | `BottomRight`| Placement: `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight`, `Center`. |
| `-Scale`        | `float` 0.01–1.0  | `0.2`          | Watermark size as a fraction of the shorter side of the base image. |
| `-PassThru`     | `switch`          |                | Returns a `FileInfo` for the output file. |

#### Examples

```powershell
# Apply a watermark to a single image (overwrites in place)
Add-ImageWatermark -Path photo.jpg -WatermarkPath logo.png

# Save to a different file, centered, at 30 % opacity
Add-ImageWatermark -Path photo.jpg -WatermarkPath logo.png `
    -Destination photo_watermarked.jpg -Position Center -Opacity 0.3

# Batch watermark every JPEG in a directory and return the output FileInfo objects
Get-ChildItem .\images -Filter *.jpg |
    Add-ImageWatermark -WatermarkPath logo.png -PassThru

# Scale the watermark to 10 % of the shorter side and place it top-right
Add-ImageWatermark -Path photo.jpg -WatermarkPath logo.png `
    -Position TopRight -Scale 0.1
```

## Running Tests

```powershell
dotnet test tests/PSImageSharp.Tests/PSImageSharp.Tests.csproj
```
