#
# Module manifest for module 'PSImageSharp'
#

@{
    # Module metadata
    ModuleVersion     = '1.0.0'
    GUID              = 'a3f7c2e1-4b8d-4f9a-bc12-3e5d7f8a9c01'
    Author            = 'PSImageSharp Contributors'
    CompanyName       = 'Community'
    Copyright         = '(c) PSImageSharp Contributors. All rights reserved.'
    Description       = 'A PowerShell module wrapping SixLabors.ImageSharp for pipeline-friendly image processing. Supports open, resize, edit, and save operations.'
    PowerShellVersion = '7.0'

    # Binary module containing the cmdlets
    RootModule        = 'PSImageSharp.dll'

    # The binary assembly that contains the cmdlets
    RequiredAssemblies = @('SixLabors.ImageSharp.dll')

    # Custom type and format data
    FormatsToProcess  = @('PSImageSharp.Format.ps1xml')

    # Cmdlets exported from this module
    CmdletsToExport   = @(
        'Open-Image'
        'Save-Image'
        'Resize-Image'
        'Edit-Image'
    )

    FunctionsToExport = @()
    AliasesToExport   = @()
    VariablesToExport = @()

    # Private data (used by PowerShellGet / PSGallery)
    PrivateData = @{
        PSData = @{
            # Tags help users discover this module via PowerShellGet.
            Tags         = @('Image', 'ImageSharp', 'SixLabors', 'PNG', 'JPEG', 'WebP', 'Resize', 'Graphics')
            LicenseUri   = 'https://github.com/SixLabors/ImageSharp/blob/main/LICENSE'
            ProjectUri   = 'https://github.com/esrklncc793/PSImageSharp'
            ReleaseNotes = 'Initial release: Open-Image, Save-Image, Resize-Image, Edit-Image cmdlets.'
        }
    }
}
