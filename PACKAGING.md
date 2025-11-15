# InstallVibe - MSIX Packaging Instructions

## Overview

This document provides complete instructions for packaging InstallVibe as an MSIX installer for Windows 10/11 distribution.

---

## Prerequisites

1. **Visual Studio 2022** with:
   - .NET Desktop Development workload
   - Windows App SDK / WinUI 3 components

2. **Windows SDK** 10.0.19041.0 or later

3. **Code Signing Certificate** (for production):
   - Self-signed certificate (for testing)
   - Purchased certificate from CA (for production)

---

## Project Configuration

### 1. Update Package.appxmanifest

Locate `/InstallVibe/Package.appxmanifest` and update the following sections:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Package
  xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
  xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
  xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
  IgnorableNamespaces="uap rescap">

  <Identity
    Name="InstallVibe"
    Publisher="CN=YourCompanyName"
    Version="1.0.0.0" />

  <Properties>
    <DisplayName>InstallVibe</DisplayName>
    <PublisherDisplayName>Your Company Name</PublisherDisplayName>
    <Logo>Assets\StoreLogo.png</Logo>
    <Description>Guided installation assistant for technicians. Zero guesswork.</Description>
  </Properties>

  <Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.19041.0" MaxVersionTested="10.0.22621.0" />
  </Dependencies>

  <Resources>
    <Resource Language="en-us" />
  </Resources>

  <Applications>
    <Application Id="App" Executable="$targetnametoken$.exe" EntryPoint="$targetentrypoint$">
      <uap:VisualElements
        DisplayName="InstallVibe"
        Description="Guided Installations. Zero Guesswork."
        BackgroundColor="transparent"
        Square150x150Logo="Assets\Square150x150Logo.png"
        Square44x44Logo="Assets\Square44x44Logo.png">
        <uap:DefaultTile
          Wide310x150Logo="Assets\Wide310x150Logo.png"
          Square71x71Logo="Assets\Square71x71Logo.png"
          Square310x310Logo="Assets\Square310x310Logo.png"
          ShortName="InstallVibe">
          <uap:ShowNameOnTiles>
            <uap:ShowOn Tile="square150x150Logo"/>
            <uap:ShowOn Tile="wide310x150Logo"/>
            <uap:ShowOn Tile="square310x310Logo"/>
          </uap:ShowNameOnTiles>
        </uap:DefaultTile>
        <uap:SplashScreen Image="Assets\SplashScreen.png" />
      </uap:VisualElements>
    </Application>
  </Applications>

  <Capabilities>
    <!-- Basic Capabilities -->
    <Capability Name="internetClient" />

    <!-- Optional: Unrestricted file system access (if needed for media files) -->
    <!-- <rescap:Capability Name="broadFileSystemAccess" /> -->
  </Capabilities>
</Package>
```

**Key Fields to Update:**
- `Identity/Publisher`: Change to your certificate's CN (Common Name)
- `Properties/PublisherDisplayName`: Your company or developer name
- `Identity/Version`: Update for each release (see Versioning Strategy below)

---

## 2. Create Self-Signed Certificate (Testing Only)

### Using PowerShell (Administrator):

```powershell
# Generate certificate
New-SelfSignedCertificate -Type Custom `
  -Subject "CN=YourCompanyName" `
  -KeyUsage DigitalSignature `
  -FriendlyName "InstallVibe Dev Certificate" `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

# Export certificate
$cert = Get-ChildItem -Path Cert:\CurrentUser\My | Where-Object {$_.Subject -eq "CN=YourCompanyName"}
Export-PfxCertificate -Cert $cert -FilePath "InstallVibe_TemporaryKey.pfx" -Password (ConvertTo-SecureString -String "YourPassword" -Force -AsPlainText)

# Export public key for distribution
Export-Certificate -Cert $cert -FilePath "InstallVibe.cer"
```

### Trust Certificate on Test Machine:

```powershell
# Install certificate to Trusted People store
Import-Certificate -FilePath "InstallVibe.cer" -CertStoreLocation Cert:\LocalMachine\TrustedPeople
```

---

## 3. Build and Package (Command Line)

### Option A: Using dotnet CLI

```bash
# Clean previous builds
dotnet clean InstallVibe/InstallVibe.csproj -c Release

# Restore dependencies
dotnet restore InstallVibe/InstallVibe.csproj

# Build MSIX package
dotnet publish InstallVibe/InstallVibe.csproj `
  -c Release `
  -r win10-x64 `
  -p:Platform=x64 `
  -p:WindowsPackageType=MSIX `
  -p:WindowsAppSDKSelfContained=true `
  -p:PublishProfile=win10-x64 `
  -p:PackageCertificateKeyFile="InstallVibe_TemporaryKey.pfx" `
  -p:PackageCertificatePassword="YourPassword"
```

**Output Location:**
```
InstallVibe/bin/x64/Release/net7.0-windows10.0.19041.0/win10-x64/AppPackages/
```

### Option B: Using MSBuild

```bash
msbuild InstallVibe.sln `
  /p:Configuration=Release `
  /p:Platform=x64 `
  /p:AppxPackageDir=".\AppPackages\" `
  /p:AppxBundle=Always `
  /p:UapAppxPackageBuildMode=StoreUpload `
  /p:PackageCertificateKeyFile="InstallVibe_TemporaryKey.pfx" `
  /p:PackageCertificatePassword="YourPassword"
```

---

## 4. Sign the MSIX Package (Post-Build)

If the package wasn't signed during build:

```bash
signtool sign /fd SHA256 `
  /a /f InstallVibe_TemporaryKey.pfx `
  /p YourPassword `
  "InstallVibe\bin\x64\Release\net7.0-windows10.0.19041.0\win10-x64\AppPackages\InstallVibe_1.0.0.0_x64.msix"
```

---

## 5. Testing Installation

### Install MSIX Package:

```powershell
# Double-click the .msix file, or use PowerShell:
Add-AppxPackage -Path ".\InstallVibe_1.0.0.0_x64.msix"
```

### Uninstall Package:

```powershell
Get-AppxPackage -Name "InstallVibe" | Remove-AppxPackage
```

### View Installed Apps:

```powershell
Get-AppxPackage | Where-Object {$_.Name -like "*InstallVibe*"}
```

---

## 6. Production Certificate (Purchased CA)

For production releases, obtain a code signing certificate from a trusted CA:

**Recommended Providers:**
- DigiCert
- Sectigo (formerly Comodo)
- GlobalSign

**Certificate Requirements:**
- **Type**: Code Signing Certificate (EV recommended for immediate SmartScreen trust)
- **Key Length**: 2048-bit RSA minimum
- **Hash Algorithm**: SHA-256

**Once obtained:**
1. Import .pfx file to your certificate store
2. Update `PackageCertificateKeyFile` in project file
3. Rebuild and sign package

---

## 7. Distribution Options

### Option A: Direct Distribution (Sideloading)
1. Distribute `.msix` file + `.cer` file
2. Users install certificate to Trusted People store
3. Users double-click `.msix` to install

### Option B: Microsoft Store
1. Create Microsoft Partner Center account
2. Submit MSIX package for validation
3. Store handles certificate trust automatically

### Option C: Enterprise Deployment
1. Use Microsoft Intune or SCCM
2. Deploy via Group Policy
3. Pre-install certificate via domain policy

---

## 8. Version Bumping Automation

### Update version in `.csproj`:

```xml
<PropertyGroup>
  <Version>1.0.1</Version>
  <ApplicationVersion>1.0.1.0</ApplicationVersion>
</PropertyGroup>
```

### Update version in Package.appxmanifest:

```xml
<Identity Version="1.0.1.0" ... />
```

**PowerShell Script to Auto-Increment:**

```powershell
# Read current version
$manifest = [xml](Get-Content "InstallVibe/Package.appxmanifest")
$currentVersion = $manifest.Package.Identity.Version
$parts = $currentVersion.Split('.')
$newBuild = [int]$parts[2] + 1
$newVersion = "$($parts[0]).$($parts[1]).$newBuild.$($parts[3])"

# Update manifest
$manifest.Package.Identity.Version = $newVersion
$manifest.Save("InstallVibe/Package.appxmanifest")

Write-Host "Version updated to: $newVersion"
```

---

## Troubleshooting

### Issue: "Package failed to install"
- **Solution**: Ensure certificate is trusted (install .cer to Trusted People)
- **Solution**: Check version number is higher than installed version

### Issue: "Publisher name mismatch"
- **Solution**: Verify `Publisher` in Package.appxmanifest matches certificate CN

### Issue: "Missing dependencies"
- **Solution**: Ensure `WindowsAppSDKSelfContained=true` in publish command

### Issue: "App crashes on launch"
- **Solution**: Check all dependencies are included (run `dumpbin /dependents` on .exe)
- **Solution**: Test in Release mode locally before packaging

---

## Checklist

- [ ] Icon assets created and placed in Assets/ folder
- [ ] Package.appxmanifest updated with correct identity and version
- [ ] Certificate generated (self-signed for testing, CA for production)
- [ ] Certificate trusted on test machine
- [ ] Build succeeds without errors
- [ ] MSIX package generated
- [ ] Package signed with certificate
- [ ] Installation tested on clean Windows machine
- [ ] App launches successfully after installation
- [ ] Uninstallation works correctly

---

**Last Updated:** 2025-01-15
**Tested On:** Windows 10 (19041), Windows 11 (22621)
