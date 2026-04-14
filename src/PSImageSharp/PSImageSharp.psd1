@{
    RootModule        = 'PSImageSharp.dll'
    ModuleVersion     = '1.0.0'
    GUID              = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    Author            = 'PSImageSharp Contributors'
    Description       = 'PowerShell module for image manipulation using SixLabors.ImageSharp'
    PowerShellVersion = '7.0'

    FormatsToProcess  = @('PSImageSharp.Format.ps1xml')

    CmdletsToExport   = @(
        'Add-ImageWatermark'
    )

    FunctionsToExport = @()
    AliasesToExport   = @()
    VariablesToExport = @()
}
