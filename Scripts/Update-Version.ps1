<#
.SYNOPSIS
    Updates version numbers across all InstallVibe project files

.DESCRIPTION
    Increments version according to semantic versioning and updates:
    - version.json
    - Package.appxmanifest
    - InstallVibe.appinstaller
    - AssemblyInfo or project file

.PARAMETER VersionPart
    Which part of the version to increment: Major, Minor, Patch, or Build

.PARAMETER SetVersion
    Explicitly set version instead of incrementing

.EXAMPLE
    .\Update-Version.ps1 -VersionPart Patch
    Increments patch version: 1.0.0 -> 1.0.1

.EXAMPLE
    .\Update-Version.ps1 -VersionPart Minor
    Increments minor version and resets patch: 1.0.5 -> 1.1.0

.EXAMPLE
    .\Update-Version.ps1 -SetVersion "2.0.0"
    Sets version explicitly to 2.0.0
#>

[CmdletBinding()]
param(
    [Parameter(ParameterSetName='Increment')]
    [ValidateSet('Major', 'Minor', 'Patch', 'Build')]
    [string]$VersionPart,

    [Parameter(ParameterSetName='Set')]
    [string]$SetVersion
)

$ErrorActionPreference = "Stop"

# Paths
$versionJsonPath = "version.json"
$manifestPath = "InstallVibe.Package/Package.appxmanifest"
$appInstallerPath = "InstallVibe.Package/InstallVibe.appinstaller"
$projectPath = "InstallVibe/InstallVibe.csproj"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "InstallVibe Version Updater" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan

# Read current version
if (-not (Test-Path $versionJsonPath)) {
    Write-Error "version.json not found at: $versionJsonPath"
    exit 1
}

$versionData = Get-Content $versionJsonPath | ConvertFrom-Json

Write-Host "Current version: $($versionData.fullVersion)" -ForegroundColor Yellow

# Calculate new version
if ($SetVersion) {
    # Explicit version set
    if ($SetVersion -notmatch '^\d+\.\d+\.\d+$') {
        Write-Error "Version must be in format Major.Minor.Patch (e.g., 1.2.3)"
        exit 1
    }

    $parts = $SetVersion.Split('.')
    $newMajor = [int]$parts[0]
    $newMinor = [int]$parts[1]
    $newPatch = [int]$parts[2]
    $newBuild = 0

    Write-Host "Setting version to: $SetVersion" -ForegroundColor Green
}
else {
    # Increment version
    $newMajor = $versionData.major
    $newMinor = $versionData.minor
    $newPatch = $versionData.patch
    $newBuild = $versionData.build

    switch ($VersionPart) {
        'Major' {
            $newMajor++
            $newMinor = 0
            $newPatch = 0
            $newBuild = 0
        }
        'Minor' {
            $newMinor++
            $newPatch = 0
            $newBuild = 0
        }
        'Patch' {
            $newPatch++
            $newBuild = 0
        }
        'Build' {
            $newBuild++
        }
    }

    Write-Host "Incrementing $VersionPart version" -ForegroundColor Green
}

$newVersion = "$newMajor.$newMinor.$newPatch"
$newFullVersion = "$newMajor.$newMinor.$newPatch.$newBuild"
$newVersionTag = "v$newVersion"

Write-Host "New version: $newFullVersion" -ForegroundColor Cyan

# Update version.json
Write-Host ""
Write-Host "[1/4] Updating version.json..." -ForegroundColor Green

$versionData.version = $newVersion
$versionData.major = $newMajor
$versionData.minor = $newMinor
$versionData.patch = $newPatch
$versionData.build = $newBuild
$versionData.fullVersion = $newFullVersion
$versionData.versionTag = $newVersionTag
$versionData.releaseDate = (Get-Date -Format "yyyy-MM-dd")

$versionData | ConvertTo-Json -Depth 10 | Set-Content $versionJsonPath
Write-Host "  ✓ Updated $versionJsonPath" -ForegroundColor Gray

# Update Package.appxmanifest
Write-Host "[2/4] Updating Package.appxmanifest..." -ForegroundColor Green

if (Test-Path $manifestPath) {
    $manifest = Get-Content $manifestPath -Raw
    $manifest = $manifest -replace 'Version="[\d\.]+"', "Version=`"$newFullVersion`""
    Set-Content $manifestPath $manifest
    Write-Host "  ✓ Updated $manifestPath" -ForegroundColor Gray
}
else {
    Write-Warning "Package.appxmanifest not found - skipping"
}

# Update InstallVibe.appinstaller
Write-Host "[3/4] Updating InstallVibe.appinstaller..." -ForegroundColor Green

if (Test-Path $appInstallerPath) {
    $appInstaller = Get-Content $appInstallerPath -Raw
    $appInstaller = $appInstaller -replace 'Version="[\d\.]+"', "Version=`"$newFullVersion`""
    Set-Content $appInstallerPath $appInstaller
    Write-Host "  ✓ Updated $appInstallerPath" -ForegroundColor Gray
}
else {
    Write-Warning "InstallVibe.appinstaller not found - skipping"
}

# Update project file
Write-Host "[4/4] Updating InstallVibe.csproj..." -ForegroundColor Green

if (Test-Path $projectPath) {
    $project = Get-Content $projectPath -Raw

    # Update or add Version element
    if ($project -match '<Version>[\d\.]+</Version>') {
        $project = $project -replace '<Version>[\d\.]+</Version>', "<Version>$newVersion</Version>"
    }
    elseif ($project -match '<PropertyGroup>') {
        $project = $project -replace '(<PropertyGroup>)', "`$1`n    <Version>$newVersion</Version>"
    }

    # Update or add AssemblyVersion
    if ($project -match '<AssemblyVersion>[\d\.]+</AssemblyVersion>') {
        $project = $project -replace '<AssemblyVersion>[\d\.]+</AssemblyVersion>', "<AssemblyVersion>$newFullVersion</AssemblyVersion>"
    }

    # Update or add FileVersion
    if ($project -match '<FileVersion>[\d\.]+</FileVersion>') {
        $project = $project -replace '<FileVersion>[\d\.]+</FileVersion>', "<FileVersion>$newFullVersion</FileVersion>"
    }

    Set-Content $projectPath $project
    Write-Host "  ✓ Updated $projectPath" -ForegroundColor Gray
}
else {
    Write-Warning "InstallVibe.csproj not found - skipping"
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Version Update Complete!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "New Version: $newFullVersion" -ForegroundColor Yellow
Write-Host "Version Tag: $newVersionTag" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review changes: git diff" -ForegroundColor Gray
Write-Host "  2. Commit changes: git add . && git commit -m 'Bump version to $newVersion'" -ForegroundColor Gray
Write-Host "  3. Create tag: git tag $newVersionTag" -ForegroundColor Gray
Write-Host "  4. Push changes: git push && git push --tags" -ForegroundColor Gray
Write-Host ""

exit 0
