#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds the PSImageSharp binary module and assembles the output module directory.

.DESCRIPTION
    1. Restores NuGet packages for the C# project.
    2. Compiles the C# project in Release configuration.
    3. Copies the required DLLs, the manifest (.psd1), and the loader (.psm1)
       into the output/PSImageSharp folder, which is a fully self-contained
       PowerShell module directory ready for Import-Module.

.EXAMPLE
    ./Build.ps1
    # Outputs module to: output/PSImageSharp/

.EXAMPLE
    ./Build.ps1 -Configuration Debug
#>

[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$repoRoot   = $PSScriptRoot
$srcDir     = Join-Path $repoRoot 'src' 'PSImageSharp'
$outputDir  = Join-Path $repoRoot 'output' 'PSImageSharp'

Write-Host "Building PSImageSharp ($Configuration)..." -ForegroundColor Cyan

# Build the C# project
Push-Location $srcDir
try {
    dotnet build --configuration $Configuration --nologo
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }
} finally {
    Pop-Location
}

# Locate build artifacts
$buildOutput = Join-Path $srcDir 'bin' $Configuration 'net8.0'

# Ensure output module directory exists and is clean
if (Test-Path $outputDir) {
    Get-ChildItem -Path $outputDir -Recurse -File |
        Where-Object { $_.Extension -in '.dll', '.pdb', '.deps.json' } |
        Remove-Item -Force
}

# Copy binary dependencies
$filesToCopy = @(
    'PSImageSharp.dll'
    'SixLabors.ImageSharp.dll'
)

foreach ($file in $filesToCopy) {
    $src  = Join-Path $buildOutput $file
    $dest = Join-Path $outputDir   $file
    if (Test-Path $src) {
        Copy-Item -Path $src -Destination $dest -Force
        Write-Verbose "Copied: $file"
    } else {
        Write-Warning "Expected build output not found: $src"
    }
}

# Optionally copy PDB for better stack traces
$pdbSrc = Join-Path $buildOutput 'PSImageSharp.pdb'
if (Test-Path $pdbSrc) {
    Copy-Item -Path $pdbSrc -Destination (Join-Path $outputDir 'PSImageSharp.pdb') -Force
}

Write-Host "`nModule built successfully." -ForegroundColor Green
Write-Host "Module directory: $outputDir" -ForegroundColor Green
Write-Host "`nTo use the module in this session:" -ForegroundColor Yellow
Write-Host "  Import-Module '$outputDir'" -ForegroundColor Yellow
