#Requires -Version 7.0

<#
.SYNOPSIS
    Builds the PSImageSharp module and copies output to ./output/PSImageSharp/.
.PARAMETER Configuration
    Build configuration: Debug or Release (default: Release).
.PARAMETER Clean
    When specified, removes the output directory before building.
#>
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [switch]$Clean
)

$ErrorActionPreference = 'Stop'

$repoRoot    = $PSScriptRoot
$projectPath = Join-Path $repoRoot 'src' 'PSImageSharp' 'PSImageSharp.csproj'
$outputPath  = Join-Path $repoRoot 'output' 'PSImageSharp'
$srcDir      = Join-Path $repoRoot 'src' 'PSImageSharp'

if ($Clean -and (Test-Path $outputPath)) {
    Write-Host "Cleaning $outputPath ..."
    Remove-Item -Recurse -Force $outputPath
}

New-Item -ItemType Directory -Force -Path $outputPath | Out-Null

Write-Host "Building PSImageSharp ($Configuration) ..."
dotnet publish $projectPath `
    --configuration $Configuration `
    --output $outputPath `
    --no-self-contained

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

# Copy module manifest and format file (not included in dotnet publish output).
foreach ($file in @('PSImageSharp.psd1', 'PSImageSharp.Format.ps1xml')) {
    $src  = Join-Path $srcDir $file
    $dest = Join-Path $outputPath $file
    Copy-Item -Path $src -Destination $dest -Force
    Write-Host "Copied $file to output."
}

Write-Host ""
Write-Host "Build complete. Module is at: $outputPath"
Write-Host "Import with: Import-Module $outputPath"
