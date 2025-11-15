<#
.SYNOPSIS
    Builds a portable (non-MSIX) distribution of InstallVibe

.DESCRIPTION
    Creates a self-contained ZIP distribution for environments that don't allow MSIX.
    Includes all runtime dependencies, generates SHA256 manifest for integrity checking.

.PARAMETER Configuration
    Build configuration (Release or Debug). Default: Release

.PARAMETER Platform
    Target platform (x64 or ARM64). Default: x64

.PARAMETER OutputPath
    Output directory for portable build. Default: .\Portable

.PARAMETER IncludeSymbols
    Include PDB debug symbols. Default: $false

.EXAMPLE
    .\Build-Portable.ps1 -Configuration Release -Platform x64

.EXAMPLE
    .\Build-Portable.ps1 -Platform ARM64 -IncludeSymbols
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',

    [Parameter()]
    [ValidateSet('x64', 'ARM64')]
    [string]$Platform = 'x64',

    [Parameter()]
    [string]$OutputPath = '.\Portable',

    [Parameter()]
    [switch]$IncludeSymbols
)

$ErrorActionPreference = "Stop"

# Configuration
$projectPath = "InstallVibe\InstallVibe.csproj"
$solutionPath = "InstallVibe.sln"
$publishDir = Join-Path $OutputPath "publish-temp"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Building Portable InstallVibe" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Platform: $Platform" -ForegroundColor Yellow
Write-Host "Output: $OutputPath" -ForegroundColor Yellow
Write-Host ""

# Ensure output directory exists
if (-not (Test-Path $OutputPath)) {
    New-Item -Path $OutputPath -ItemType Directory | Out-Null
}

# Step 1: Restore dependencies
Write-Host "[1/6] Restoring dependencies..." -ForegroundColor Green
dotnet restore $solutionPath
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to restore dependencies"
    exit 1
}

# Step 2: Build solution
Write-Host "[2/6] Building solution..." -ForegroundColor Green
dotnet build $solutionPath -c $Configuration -p:Platform=$Platform
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build solution"
    exit 1
}

# Step 3: Publish self-contained
Write-Host "[3/6] Publishing self-contained application..." -ForegroundColor Green

$runtime = if ($Platform -eq 'x64') { 'win-x64' } else { 'win-arm64' }

$publishArgs = @(
    'publish',
    $projectPath,
    '-c', $Configuration,
    '-r', $runtime,
    '--self-contained', 'true',
    '-p:PublishSingleFile=false',  # Use folder structure for better compatibility
    '-p:PublishReadyToRun=true',   # AOT compilation for better performance
    '-p:PublishTrimmed=false',     # Don't trim for WinUI (can cause issues)
    '-o', $publishDir
)

& dotnet $publishArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to publish application"
    exit 1
}

# Step 4: Organize portable package
Write-Host "[4/6] Organizing portable package..." -ForegroundColor Green

$version = (Get-Content "InstallVibe/InstallVibe.csproj" | Select-String -Pattern '<Version>(.*)</Version>').Matches[0].Groups[1].Value
if (-not $version) {
    $version = "1.0.0"
}

$packageName = "InstallVibe_${version}_${Platform}_Portable"
$packageDir = Join-Path $OutputPath $packageName

if (Test-Path $packageDir) {
    Remove-Item $packageDir -Recurse -Force
}

New-Item -Path $packageDir -ItemType Directory | Out-Null

# Copy published files
Copy-Item -Path "$publishDir\*" -Destination $packageDir -Recurse -Force

# Remove PDB files if not including symbols
if (-not $IncludeSymbols) {
    Get-ChildItem -Path $packageDir -Filter *.pdb -Recurse | Remove-Item -Force
    Write-Host "  Removed debug symbols" -ForegroundColor Gray
}

# Step 5: Create additional files
Write-Host "[5/6] Creating distribution files..." -ForegroundColor Green

# Create README.txt
$readmeContent = @"
============================================================
InstallVibe ${version} - Portable Distribution
============================================================

This is a portable, self-contained distribution of InstallVibe
that does not require installation or MSIX support.

SYSTEM REQUIREMENTS:
- Windows 10 version 1809 (Build 17763) or later
- $Platform processor architecture
- .NET runtime is included (no separate installation needed)

INSTALLATION:
1. Extract this ZIP to any folder (e.g., C:\InstallVibe)
2. Run InstallVibe.exe
3. (Optional) Create desktop shortcut

FEATURES:
- Fully self-contained (includes all dependencies)
- No installation required
- No administrator privileges needed
- Portable - run from USB drive or network share
- All data stored in local AppData folder

DATA LOCATION:
Application data is stored in:
%LOCALAPPDATA%\InstallVibe

TROUBLESHOOTING:
- If app doesn't start, check Windows Event Viewer
- Ensure antivirus allows execution
- Verify file integrity using verify.ps1

SUPPORT:
For support, contact your IT department or visit:
https://github.com/yourcompany/installvibe

VERSION: ${version}
BUILD DATE: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
PLATFORM: ${Platform}
CONFIGURATION: ${Configuration}
============================================================
"@

Set-Content -Path (Join-Path $packageDir "README.txt") -Value $readmeContent

# Create verification script
$verifyScript = @'
# Verify.ps1 - Verify InstallVibe portable package integrity

$manifestPath = "SHA256SUMS.txt"

if (-not (Test-Path $manifestPath)) {
    Write-Error "SHA256SUMS.txt not found"
    exit 1
}

Write-Host "Verifying package integrity..." -ForegroundColor Cyan

$failures = 0
$successes = 0

Get-Content $manifestPath | ForEach-Object {
    if ($_ -match '^([a-f0-9]+)\s+(.+)$') {
        $expectedHash = $matches[1]
        $file = $matches[2]

        if (Test-Path $file) {
            $actualHash = (Get-FileHash $file -Algorithm SHA256).Hash.ToLower()

            if ($actualHash -eq $expectedHash) {
                Write-Host "✓ $file" -ForegroundColor Green
                $script:successes++
            } else {
                Write-Host "✗ $file (hash mismatch)" -ForegroundColor Red
                $script:failures++
            }
        } else {
            Write-Host "✗ $file (not found)" -ForegroundColor Yellow
            $script:failures++
        }
    }
}

Write-Host ""
Write-Host "Verified $successes files successfully" -ForegroundColor Green
if ($failures -gt 0) {
    Write-Host "$failures files failed verification" -ForegroundColor Red
    exit 1
}

Write-Host "All files verified successfully!" -ForegroundColor Green
exit 0
'@

Set-Content -Path (Join-Path $packageDir "Verify.ps1") -Value $verifyScript

# Create launcher script (optional convenience)
$launchScript = @'
@echo off
REM Launch InstallVibe portable

echo Starting InstallVibe...
start "" "%~dp0InstallVibe.exe"

REM Uncomment to keep console open for debugging
REM pause
'@

Set-Content -Path (Join-Path $packageDir "Launch.bat") -Value $launchScript

# Step 6: Generate SHA256 manifest
Write-Host "[6/6] Generating SHA256 integrity manifest..." -ForegroundColor Green

$manifestPath = Join-Path $packageDir "SHA256SUMS.txt"
$manifestContent = @()

Get-ChildItem -Path $packageDir -File -Recurse | Where-Object { $_.Name -ne "SHA256SUMS.txt" } | ForEach-Object {
    $hash = (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLower()
    $relativePath = $_.FullName.Substring($packageDir.Length + 1)
    $manifestContent += "$hash  $relativePath"
}

Set-Content -Path $manifestPath -Value ($manifestContent | Sort-Object)

Write-Host "  Generated manifest for $($manifestContent.Count) files" -ForegroundColor Gray

# Create ZIP archive
Write-Host ""
Write-Host "Creating ZIP archive..." -ForegroundColor Green

$zipPath = Join-Path $OutputPath "$packageName.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive -Path $packageDir -DestinationPath $zipPath -CompressionLevel Optimal

# Calculate ZIP hash
$zipHash = (Get-FileHash $zipPath -Algorithm SHA256).Hash
$zipSize = (Get-Item $zipPath).Length / 1MB

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Portable Build Complete!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Package: $zipPath" -ForegroundColor Yellow
Write-Host "Size: $([math]::Round($zipSize, 2)) MB" -ForegroundColor Yellow
Write-Host "SHA256: $zipHash" -ForegroundColor Yellow
Write-Host ""
Write-Host "Distribution includes:" -ForegroundColor Cyan
Write-Host "  - Self-contained application" -ForegroundColor Gray
Write-Host "  - All runtime dependencies" -ForegroundColor Gray
Write-Host "  - README.txt with instructions" -ForegroundColor Gray
Write-Host "  - Verify.ps1 integrity checker" -ForegroundColor Gray
Write-Host "  - Launch.bat convenience launcher" -ForegroundColor Gray
Write-Host "  - SHA256SUMS.txt file manifest" -ForegroundColor Gray
Write-Host ""

# Clean up temp directory
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

Write-Host "✅ Portable distribution ready for deployment!" -ForegroundColor Green

exit 0
