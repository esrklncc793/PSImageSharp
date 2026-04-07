#Requires -Version 7.0

# PSImageSharp Module Loader
# This file imports the binary assembly and sets up type formatting.

$assemblyPath = Join-Path $PSScriptRoot 'PSImageSharp.dll'

if (-not (Test-Path $assemblyPath)) {
    throw "PSImageSharp.dll not found at '$assemblyPath'. Please build the module first."
}

Add-Type -Path $assemblyPath

# Register a custom default display format for PSImage objects so that
# Width, Height, Format, and SourcePath are shown in the console without
# the user having to pipe to Select-Object.
Update-TypeData -TypeName 'PSImageSharp.PSImage' `
    -DefaultDisplayPropertySet @('Width', 'Height', 'Format', 'SourcePath') `
    -Force
