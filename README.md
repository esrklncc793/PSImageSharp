# PSImageSharp

A high-performance PowerShell Binary Module that wraps [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) to make image processing accessible via the PowerShell pipeline.

## Features

| Cmdlet | Description |
|---|---|
| `Open-Image` | Load an image from disk into a `PSImage` pipeline object |
| `Resize-Image` | Scale by width, height, or percentage with automatic aspect-ratio handling |
| `Edit-Image` | Apply filters: grayscale, brightness, contrast, saturation, hue, flip, rotate |
| `Save-Image` | Save to PNG, JPEG, or WebP with optional quality control |

All cmdlets pass the image object through the pipeline so operations can be chained:

```powershell
Open-Image "photo.jpg" | Resize-Image -Width 800 | Save-Image "thumb.jpg"
```

---

## Prerequisites

| Component | Minimum Version |
|---|---|
| PowerShell | 7.0 |
| .NET SDK | 8.0 (build only) |

---

## Getting Started

### 1 — Clone and build

```powershell
git clone https://github.com/esrklncc793/PSImageSharp.git
cd PSImageSharp
./Build.ps1          # builds C# project and assembles output/PSImageSharp/
```

### 2 — Import the module

```powershell
Import-Module ./output/PSImageSharp
```

To make the import permanent, copy (or symlink) `output/PSImageSharp` into one of the directories in `$env:PSModulePath`.

### 3 — Verify

```powershell
Get-Command -Module PSImageSharp
```

Expected output:

```
CommandType  Name          Version  Source
-----------  ----          -------  ------
Cmdlet       Edit-Image    1.0.0    PSImageSharp
Cmdlet       Open-Image    1.0.0    PSImageSharp
Cmdlet       Resize-Image  1.0.0    PSImageSharp
Cmdlet       Save-Image    1.0.0    PSImageSharp
```

---

## Usage Examples

### Open an image

```powershell
$img = Open-Image -Path "photo.jpg"
$img   # Width Height Format SourcePath
```

You can also pipe `Get-ChildItem` results directly:

```powershell
Get-ChildItem *.jpg | Open-Image
```

### Resize — preserve aspect ratio

```powershell
# Scale to 800 px wide; height calculated automatically
Open-Image "photo.jpg" | Resize-Image -Width 800 | Save-Image "thumb.jpg"

# Scale to exactly 1920×1080 (ignores aspect ratio)
Open-Image "photo.jpg" | Resize-Image -Width 1920 -Height 1080 | Save-Image "wallpaper.jpg"

# Scale to 50 % of original size
Open-Image "photo.jpg" | Resize-Image -Percentage 50 | Save-Image "half.jpg"
```

### Apply filters

```powershell
# Greyscale
Open-Image "photo.jpg" | Edit-Image -Grayscale | Save-Image "grey.jpg"

# Brightness / contrast adjustments (range: -1.0 to 1.0; 0 = no change)
Open-Image "photo.jpg" | Edit-Image -Brightness 0.1 -Contrast 0.1 | Save-Image "vivid.jpg"

# Flip & rotate
Open-Image "photo.jpg" | Edit-Image -FlipHorizontal -RotateDegrees 90 | Save-Image "rotated.jpg"

# Auto-orient using EXIF data
Open-Image "photo.jpg" | Edit-Image -AutoOrient | Save-Image "oriented.jpg"
```

### Save with format and quality control

```powershell
# Infer format from extension (.webp)
$img | Save-Image -Path "output.webp"

# Explicit format and quality
$img | Save-Image -Path "output.jpg" -Format Jpeg -Quality 85

# Pass image through pipeline after saving
$img | Save-Image "thumbnail.jpg" -PassThru | Save-Image "thumbnail.webp"
```

### Full pipeline chain

```powershell
Open-Image "photo.jpg" |
    Edit-Image -AutoOrient |
    Resize-Image -Width 1200 |
    Edit-Image -Brightness 0.05 -Contrast 0.05 |
    Save-Image "processed.jpg" -Quality 92
```

---

## Cmdlet Reference

### `Open-Image`

```
Open-Image [-Path] <string> [<CommonParameters>]
```

| Parameter | Required | Description |
|---|---|---|
| `-Path` | Yes | File path of the image to load. Alias: `FullName` |

### `Resize-Image`

```
Resize-Image -InputObject <PSImage> [-Width <int>] [-Height <int>] [<CommonParameters>]
Resize-Image -InputObject <PSImage> -Percentage <double> [<CommonParameters>]
```

| Parameter | Description |
|---|---|
| `-Width` | Target width in pixels. When supplied alone, height is computed automatically to preserve the aspect ratio. |
| `-Height` | Target height in pixels. When supplied alone, width is computed automatically. |
| `-Width` + `-Height` | Both supplied → exact resize (aspect ratio not preserved). |
| `-Percentage` | Scale factor (e.g. `50` = half, `200` = double). |

### `Edit-Image`

```
Edit-Image -InputObject <PSImage> [-Grayscale] [-Brightness <float>] [-Contrast <float>]
           [-Saturation <float>] [-Hue <float>] [-FlipHorizontal] [-FlipVertical]
           [-RotateDegrees <int>] [-AutoOrient] [<CommonParameters>]
```

| Parameter | Range | Description |
|---|---|---|
| `-Grayscale` | switch | Convert to greyscale |
| `-Brightness` | −1.0 → 1.0 | Adjust brightness (0 = no change) |
| `-Contrast` | −1.0 → 1.0 | Adjust contrast (0 = no change) |
| `-Saturation` | −1.0 → 1.0 | Adjust colour saturation (0 = no change) |
| `-Hue` | −360 → 360 | Rotate hue by degrees (0 = no change) |
| `-FlipHorizontal` | switch | Mirror horizontally |
| `-FlipVertical` | switch | Mirror vertically |
| `-RotateDegrees` | −360 → 360 | Rotate clockwise by degrees |
| `-AutoOrient` | switch | Apply EXIF orientation and strip the tag |

### `Save-Image`

```
Save-Image [-Path] <string> -InputObject <PSImage> [-Format <string>] [-Quality <int>]
           [-PassThru] [<CommonParameters>]
```

| Parameter | Description |
|---|---|
| `-Path` | Destination file path. Extension determines format unless `-Format` is set. |
| `-Format` | `Jpeg`, `Png`, or `Webp` |
| `-Quality` | Lossy quality 1–100 (JPEG/WebP). Default: 90 |
| `-PassThru` | Emit the `PSImage` object to the pipeline after saving |

---

## Architecture

```
PSImageSharp/
├── Build.ps1                  # Build script (compiles C# → copies to output/)
├── src/
│   └── PSImageSharp/
│       ├── PSImageSharp.csproj
│       ├── PSImage.cs                  # PSImage wrapper class
│       └── Cmdlets/
│           ├── OpenImageCmdlet.cs
│           ├── SaveImageCmdlet.cs
│           ├── ResizeImageCmdlet.cs
│           └── EditImageCmdlet.cs
└── output/
    └── PSImageSharp/          # Distributable module directory
        ├── PSImageSharp.psd1  # Module manifest
        ├── PSImageSharp.dll   # Compiled binary (after Build.ps1)
        ├── SixLabors.ImageSharp.dll
        └── PSImageSharp.Format.ps1xml
```

The module is implemented as a **Binary Module** (C# compiled to a DLL). This gives:
- Full type safety and IntelliSense in the C# source
- Direct access to `SixLabors.ImageSharp`'s `Mutate` API for in-place image processing
- Proper pipeline binding via `[Parameter(ValueFromPipeline = true)]`

---

## License

PSImageSharp is released under the **MIT License**.

SixLabors.ImageSharp uses the **Six Labors Split License**:
- **Apache 2.0** for non-commercial / open-source use.
- A **commercial license** is required for closed-source commercial use.

See [SixLabors Licensing](https://sixlabors.com/pricing/) for details.
