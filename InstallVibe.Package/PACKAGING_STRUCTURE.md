# InstallVibe.Package - MSIX Packaging Structure

This document describes the folder structure and configuration for building InstallVibe MSIX packages.

## Folder Structure

```
InstallVibe.Package/
├── Package.appxmanifest                    # MSIX manifest (app identity, capabilities, protocols)
├── InstallVibe.Package.wapproj             # Packaging project file
├── InstallVibe.Package_TemporaryKey.pfx    # Development certificate (NOT for production)
├── Images/                                 # Visual assets for tiles and branding
│   ├── StoreLogo.png                       # 50x50 - Store/Apps & Features icon
│   ├── Square44x44Logo.png                 # 44x44 - App list icon
│   ├── Square44x44Logo.targetsize-*.png    # Various sizes for taskbar/start
│   ├── Square150x150Logo.png               # 150x150 - Medium tile
│   ├── Square71x71Logo.png                 # 71x71 - Small tile
│   ├── Square310x310Logo.png               # 310x310 - Large tile
│   ├── Wide310x150Logo.png                 # 310x150 - Wide tile
│   ├── SplashScreen.png                    # 620x300 - Launch splash screen
│   ├── FileTypeLogo.png                    # 256x256 - .ivguide file icon
│   └── ProtocolLogo.png                    # 256x256 - installvibe:// protocol icon
├── Properties/
│   └── PublishProfiles/
│       ├── win10-x64.pubxml                # x64 publish profile
│       └── win10-arm64.pubxml              # ARM64 publish profile
├── AppPackages/                            # Build output directory (auto-generated)
│   └── InstallVibe_1.0.0.0_Test/
│       ├── InstallVibe_1.0.0.0_x64.msix
│       ├── InstallVibe_1.0.0.0_arm64.msix
│       └── InstallVibe_1.0.0.0_x64_arm64_bundle.msixbundle
└── PACKAGING_STRUCTURE.md                  # This file
```

## Image Asset Requirements

### Required Assets and Sizes

All assets should be PNG format with transparency where appropriate.

| Asset Name | Size | Purpose | Scale Variants |
|------------|------|---------|----------------|
| StoreLogo | 50x50 | Apps & Features list | 100%, 125%, 150%, 200%, 400% |
| Square44x44Logo | 44x44 | App list, taskbar | targetsize-16, 24, 32, 48, 256 (unplated) |
| Square150x150Logo | 150x150 | Medium Start tile | 100%, 125%, 150%, 200%, 400% |
| Square71x71Logo | 71x71 | Small Start tile | 100%, 125%, 150%, 200%, 400% |
| Square310x310Logo | 310x310 | Large Start tile | 100%, 125%, 150%, 200%, 400% |
| Wide310x150Logo | 310x150 | Wide Start tile | 100%, 125%, 150%, 200%, 400% |
| SplashScreen | 620x300 | App launch screen | 100%, 125%, 150%, 200%, 400% |
| FileTypeLogo | 256x256 | .ivguide file association | - |
| ProtocolLogo | 256x256 | installvibe:// protocol | - |

### Scale Variant Naming Convention

```
Square150x150Logo.scale-100.png    # 150x150 (100% scale)
Square150x150Logo.scale-125.png    # 188x188 (125% scale)
Square150x150Logo.scale-150.png    # 225x225 (150% scale)
Square150x150Logo.scale-200.png    # 300x300 (200% scale)
Square150x150Logo.scale-400.png    # 600x600 (400% scale)
```

### Target-Size Naming (for Square44x44Logo)

```
Square44x44Logo.targetsize-16.png
Square44x44Logo.targetsize-24.png
Square44x44Logo.targetsize-32.png
Square44x44Logo.targetsize-48.png
Square44x44Logo.targetsize-256.png
Square44x44Logo.targetsize-256_altform-unplated.png  # No background padding
```

## Branding Guidelines

### Color Palette

- **Primary Brand Color**: `#0078D4` (Azure Blue)
- **Background**: `#FFFFFF` (White) or Transparent
- **Accent**: `#005A9E` (Dark Blue)
- **Success**: `#107C10` (Green)

### Logo Design Principles

1. **Clarity**: Icons must be recognizable at 16x16 pixels
2. **Contrast**: Use high contrast for accessibility
3. **Simplicity**: Avoid fine details that don't scale well
4. **Consistency**: Use same visual language across all assets
5. **Factory-Friendly**: Bold, high-contrast designs for gloved hands and bright/dim environments

## Building the Package

### Visual Studio

1. Right-click `InstallVibe.Package` project
2. Select **Publish** → **Create App Packages**
3. Choose distribution method:
   - **Sideloading**: For enterprise deployment
   - **Store**: For Microsoft Store (future)
4. Select architectures: `x64` and `ARM64`
5. Configure signing certificate
6. Click **Create**

### Command Line (MSBuild)

```bash
# Restore dependencies
dotnet restore InstallVibe.sln

# Build for x64
msbuild InstallVibe.Package/InstallVibe.Package.wapproj /p:Configuration=Release /p:Platform=x64 /p:AppxBundle=Never

# Build for ARM64
msbuild InstallVibe.Package/InstallVibe.Package.wapproj /p:Configuration=Release /p:Platform=ARM64 /p:AppxBundle=Never

# Create bundle
msbuild InstallVibe.Package/InstallVibe.Package.wapproj /p:Configuration=Release /p:AppxBundle=Always /p:AppxBundlePlatforms="x64|ARM64"
```

### PowerShell Script (Automated)

```powershell
# See Build-MSIX.ps1 in root directory
.\Build-MSIX.ps1 -Configuration Release -Platforms @("x64", "ARM64") -Sign
```

## Code Signing

### Development Certificate

For local testing, Visual Studio generates a temporary certificate (`InstallVibe.Package_TemporaryKey.pfx`).

**Trust the certificate:**
```powershell
$cert = Get-PfxCertificate -FilePath "InstallVibe.Package_TemporaryKey.pfx"
Import-Certificate -CertStoreLocation Cert:\LocalMachine\Root -Certificate $cert
```

### Production Certificate

**NEVER commit production certificates to source control.**

Production builds should use:
- Enterprise code-signing certificate
- Stored in Azure Key Vault or similar secure storage
- Injected by CI/CD pipeline at build time

See `SIGNING.md` for complete signing instructions.

## Platform-Specific Notes

### x64 (Intel/AMD 64-bit)

- **Target**: Most desktop PCs and laptops
- **Runtime**: .NET 7.0 Windows x64
- **Package Size**: ~50-80 MB (including runtime)

### ARM64 (Surface, Windows on ARM)

- **Target**: ARM-based tablets and devices
- **Runtime**: .NET 7.0 Windows ARM64
- **Package Size**: ~45-75 MB (including runtime)
- **Note**: Ensure all native dependencies have ARM64 builds

## Bundle vs. Individual Packages

### Bundle (.msixbundle)

- **Recommended for**: Enterprise deployment with mixed device types
- **Contains**: Both x64 and ARM64 packages
- **Size**: Combined size of all architectures
- **Deployment**: Windows automatically selects correct architecture

### Individual Packages (.msix)

- **Recommended for**: Homogeneous environments (all same architecture)
- **Contains**: Single architecture
- **Size**: Smaller download
- **Deployment**: Must select correct package for device type

## Troubleshooting

### "Publisher name does not match certificate"

Update `Publisher` in `Package.appxmanifest` to match certificate's Subject CN:

```bash
certutil -dump YourCertificate.pfx
# Look for "Subject: CN=Your Company Name"
```

### "Version number must be higher"

Each release must have a higher version number than previous installations.
Increment version in `Package.appxmanifest`:

```xml
<Identity Version="1.0.1.0" ... />
```

### Missing dependencies

Ensure all runtime dependencies are included:
1. .NET 7.0 Windows Runtime
2. Microsoft.WindowsAppSDK runtime
3. Visual C++ redistributables (usually bundled automatically)

## Next Steps

- Configure auto-updates: See `InstallVibe.appinstaller`
- Set up CI/CD: See `.github/workflows/release.yml`
- Code signing: See `SIGNING.md`
- Enterprise deployment: See `ENTERPRISE_DEPLOYMENT.md`
