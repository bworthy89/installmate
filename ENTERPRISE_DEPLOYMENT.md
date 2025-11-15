# InstallVibe Enterprise Deployment Guide

Complete guide for IT departments deploying InstallVibe in enterprise environments.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Certificate Installation](#certificate-installation)
3. [MSIX Installation Methods](#msix-installation-methods)
4. [Update Server Configuration](#update-server-configuration)
5. [Group Policy Deployment](#group-policy-deployment)
6. [Offline Deployment](#offline-deployment)
7. [Uninstall and Repair](#uninstall-and-repair)
8. [Troubleshooting](#troubleshooting)
9. [Security Considerations](#security-considerations)

---

## Prerequisites

### System Requirements

**Client Machines:**
- Windows 10 version 1809 (Build 17763) or later
- Windows 11 (all versions)
- Architecture: x64 or ARM64
- Disk space: 200 MB minimum
- Memory: 4 GB RAM recommended
- .NET 7.0 Runtime (included in MSIX package)

**Server Infrastructure:**
- Web server for hosting update files (IIS, Apache, Nginx)
- HTTPS recommended for update server
- Bandwidth: Estimate 50 MB per client for initial install

### Required Permissions

**IT Administrator:**
- Local Administrator rights for certificate installation
- Domain Admin for Group Policy deployment
- Web server admin access for update hosting

**End Users:**
- No administrator rights required for installation (if certificate is trusted)
- Write access to `%LOCALAPPDATA%` for app data

---

## Certificate Installation

InstallVibe MSIX packages must be signed with a trusted certificate.

### Option 1: Trust on Individual Machine (Testing)

```powershell
# Run as Administrator
$cert = Get-PfxCertificate -FilePath "\\server\share\InstallVibe-Cert.cer"
Import-Certificate -CertStoreLocation Cert:\LocalMachine\Root -Certificate $cert
```

### Option 2: Trust via Group Policy (Recommended for Enterprises)

1. **Open Group Policy Management Console** (`gpmc.msc`)

2. **Navigate to:**
   ```
   Computer Configuration
   └── Policies
       └── Windows Settings
           └── Security Settings
               └── Public Key Policies
                   └── Trusted Root Certification Authorities
   ```

3. **Import Certificate:**
   - Right-click → **Import**
   - Browse to `InstallVibe-Cert.cer`
   - Complete wizard

4. **Apply Policy:**
   ```cmd
   gpupdate /force
   ```

5. **Verify (on client):**
   ```powershell
   Get-ChildItem Cert:\LocalMachine\Root | Where-Object {$_.Subject -like "*YourCompany*"}
   ```

### Option 3: Trust via PowerShell Script (Remote Deployment)

```powershell
# Deploy-Certificate.ps1
# Run on each client via SCCM, Intune, or remote PowerShell

$certUrl = "https://internal-server/certs/InstallVibe-Cert.cer"
$certPath = Join-Path $env:TEMP "InstallVibe-Cert.cer"

# Download certificate
Invoke-WebRequest -Uri $certUrl -OutFile $certPath

# Install certificate
Import-Certificate -FilePath $certPath -CertStoreLocation Cert:\LocalMachine\Root

# Clean up
Remove-Item $certPath

Write-Host "Certificate installed successfully"
```

---

## MSIX Installation Methods

### Method 1: Direct Installation (Double-Click)

**For individual users or small deployments:**

1. Download `InstallVibe_1.0.0.0_bundle.msixbundle` to user machine
2. Double-click the file
3. Click **Install**
4. Launch from Start Menu

**Prerequisite:** Certificate must already be trusted.

### Method 2: PowerShell Installation

**For scripted deployment:**

```powershell
# Install-InstallVibe.ps1

$packagePath = "\\fileserver\software\InstallVibe\InstallVibe_1.0.0.0_bundle.msixbundle"

# Install package
Add-AppxPackage -Path $packagePath

# Verify installation
$app = Get-AppxPackage -Name "InstallVibe"
if ($app) {
    Write-Host "✓ InstallVibe $($app.Version) installed successfully"
}
else {
    Write-Error "Installation failed"
    exit 1
}
```

### Method 3: App Installer (With Auto-Updates)

**Recommended for managed deployments:**

1. **Host appinstaller file on internal server**
   - Upload `InstallVibe.appinstaller` to `https://internal-updates.company.com/installvibe/`
   - Update URLs in appinstaller to match your server

2. **Install via App Installer link:**
   ```powershell
   Start-Process "https://internal-updates.company.com/installvibe/InstallVibe.appinstaller"
   ```

3. **Silent installation:**
   ```cmd
   start ms-appinstaller:?source=https://internal-updates.company.com/installvibe/InstallVibe.appinstaller
   ```

### Method 4: Group Policy / SCCM / Intune

**Microsoft Endpoint Manager (Intune):**

1. Navigate to **Apps** → **Windows**
2. Click **Add** → **Line-of-business app**
3. Upload `InstallVibe_1.0.0.0_bundle.msixbundle`
4. Configure app information
5. Assign to groups
6. Deploy

**SCCM (Configuration Manager):**

1. Create new application
2. Select deployment type: **Windows app package (.appx, .appxbundle)**
3. Specify `InstallVibe_1.0.0.0_bundle.msixbundle`
4. Configure detection method
5. Distribute content
6. Deploy to collection

---

## Update Server Configuration

### IIS Configuration

**1. Create Web Directory:**
```powershell
# Run as Administrator
New-Item -Path "C:\inetpub\wwwroot\installvibe" -ItemType Directory

# Copy files
Copy-Item "\\build-server\releases\latest\*" "C:\inetpub\wwwroot\installvibe\" -Recurse
```

**2. Configure MIME Types:**
```powershell
Import-Module WebAdministration

# Add MIME types for MSIX files
Add-WebConfigurationProperty -PSPath "IIS:\Sites\Default Web Site\installvibe" `
    -Filter "system.webServer/staticContent" `
    -Name "." `
    -Value @{fileExtension='.appinstaller'; mimeType='application/xml'}

Add-WebConfigurationProperty -PSPath "IIS:\Sites\Default Web Site\installvibe" `
    -Filter "system.webServer/staticContent" `
    -Name "." `
    -Value @{fileExtension='.msix'; mimeType='application/octet-stream'}

Add-WebConfigurationProperty -PSPath "IIS:\Sites\Default Web Site\installvibe" `
    -Filter "system.webServer/staticContent" `
    -Name "." `
    -Value @{fileExtension='.msixbundle'; mimeType='application/octet-stream'}
```

**3. Enable HTTPS (Recommended):**
```powershell
# Obtain SSL certificate (from CA or Let's Encrypt)
# Bind to site
New-WebBinding -Name "Default Web Site" -Protocol https -Port 443
```

**4. Configure CORS (if needed):**
```xml
<!-- web.config in C:\inetpub\wwwroot\installvibe -->
<configuration>
  <system.webServer>
    <httpProtocol>
      <customHeaders>
        <add name="Access-Control-Allow-Origin" value="*" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
</configuration>
```

### Update URL Configuration

**Update InstallVibe.appinstaller URLs:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<AppInstaller
    Version="1.0.0.0"
    Uri="https://internal-updates.company.com/installvibe/InstallVibe.appinstaller">

  <MainBundle
      Version="1.0.0.0"
      Uri="https://internal-updates.company.com/installvibe/InstallVibe_1.0.0.0_bundle.msixbundle" />

  <UpdateSettings>
    <OnLaunch HoursBetweenUpdateChecks="24" ShowPrompt="true" UpdateBlocksActivation="false" />
  </UpdateSettings>
</AppInstaller>
```

**Test accessibility:**
```powershell
Invoke-WebRequest -Uri "https://internal-updates.company.com/installvibe/InstallVibe.appinstaller"
```

---

## Group Policy Deployment

### Create GPO for InstallVibe

**1. Create Installation Script:**

Save as `\\domain\netlogon\InstallVibe-Deploy.ps1`:

```powershell
# InstallVibe deployment script

$packageUrl = "https://internal-updates.company.com/installvibe/InstallVibe.appinstaller"
$logFile = "C:\ProgramData\InstallVibe\install.log"

try {
    # Create log directory
    New-Item -Path "C:\ProgramData\InstallVibe" -ItemType Directory -Force | Out-Null

    # Check if already installed
    $installed = Get-AppxPackage -Name "InstallVibe"
    if ($installed) {
        "$(Get-Date) - InstallVibe already installed (version $($installed.Version))" | Out-File $logFile -Append
        exit 0
    }

    # Install via App Installer
    "$(Get-Date) - Installing InstallVibe from $packageUrl" | Out-File $logFile -Append
    Start-Process "ms-appinstaller:?source=$packageUrl" -Wait

    # Verify
    $installed = Get-AppxPackage -Name "InstallVibe"
    if ($installed) {
        "$(Get-Date) - InstallVibe installed successfully (version $($installed.Version))" | Out-File $logFile -Append
        exit 0
    }
    else {
        "$(Get-Date) - Installation verification failed" | Out-File $logFile -Append
        exit 1
    }
}
catch {
    "$(Get-Date) - Error: $_" | Out-File $logFile -Append
    exit 1
}
```

**2. Create GPO:**

1. Open **Group Policy Management** (`gpmc.msc`)
2. Right-click OU → **Create a GPO in this domain, and Link it here...**
3. Name: "Deploy InstallVibe"

**3. Configure Startup Script:**

```
Computer Configuration
└── Policies
    └── Windows Settings
        └── Scripts (Startup/Shutdown)
            └── Startup
```

- Click **Add** → **PowerShell Scripts**
- Browse to `\\domain\netlogon\InstallVibe-Deploy.ps1`
- Click **OK**

**4. Apply GPO:**

```powershell
gpupdate /force
```

**5. Verify on client:**

```powershell
gpresult /h C:\gpresult.html
```

---

## Offline Deployment

For air-gapped or restricted networks without internet access.

### Preparation

**1. Download all files to USB drive:**

```
USB:\InstallVibe-Offline\
├── InstallVibe_1.0.0.0_x64_arm64_bundle.msixbundle
├── InstallVibe-Cert.cer
├── Install-Offline.ps1
└── README.txt
```

**2. Create Installation Script (`Install-Offline.ps1`):**

```powershell
# Install-Offline.ps1 - Offline InstallVibe deployment

$scriptDir = $PSScriptRoot
$certPath = Join-Path $scriptDir "InstallVibe-Cert.cer"
$packagePath = Join-Path $scriptDir "InstallVibe_1.0.0.0_x64_arm64_bundle.msixbundle"

Write-Host "InstallVibe Offline Installer" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan

# Check for admin rights
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script must be run as Administrator"
    Pause
    exit 1
}

# Install certificate
Write-Host "[1/2] Installing certificate..." -ForegroundColor Green
try {
    Import-Certificate -FilePath $certPath -CertStoreLocation Cert:\LocalMachine\Root
    Write-Host "  ✓ Certificate installed" -ForegroundColor Gray
}
catch {
    Write-Warning "Certificate installation failed (may already be installed): $_"
}

# Install package
Write-Host "[2/2] Installing InstallVibe..." -ForegroundColor Green
try {
    Add-AppxPackage -Path $packagePath
    Write-Host "  ✓ InstallVibe installed successfully" -ForegroundColor Gray
}
catch {
    Write-Error "Installation failed: $_"
    Pause
    exit 1
}

Write-Host ""
Write-Host "Installation complete!" -ForegroundColor Green
Write-Host "Launch InstallVibe from the Start Menu" -ForegroundColor Cyan
Pause
```

### Deployment Steps

**On each machine:**

1. Insert USB drive
2. Right-click `Install-Offline.ps1` → **Run with PowerShell**
3. Click **Yes** to UAC prompt
4. Wait for installation to complete

**Or via remote PowerShell:**

```powershell
# Copy files to remote machine
$session = New-PSSession -ComputerName "COMPUTER-NAME"
Copy-Item "\\fileserver\InstallVibe-Offline\*" -Destination "C:\Temp\InstallVibe" -ToSession $session -Recurse

# Execute installation
Invoke-Command -Session $session -ScriptBlock {
    Set-ExecutionPolicy Bypass -Scope Process -Force
    & "C:\Temp\InstallVibe\Install-Offline.ps1"
}

Remove-PSSession $session
```

---

## Uninstall and Repair

### Uninstall

**Via Settings:**
1. Open **Settings** → **Apps** → **Apps & features**
2. Find "InstallVibe"
3. Click **Uninstall**

**Via PowerShell:**
```powershell
Get-AppxPackage -Name "InstallVibe" | Remove-AppxPackage
```

**Via Group Policy:**

Remove GPO deployment or create removal script:

```powershell
# Uninstall-InstallVibe.ps1
Get-AppxPackage -Name "InstallVibe" | Remove-AppxPackage
```

### Repair

**Repair installation:**
```powershell
# Re-register the app
Get-AppxPackage -Name "InstallVibe" | ForEach-Object {
    Add-AppxPackage -DisableDevelopmentMode -Register "$($_.InstallLocation)\AppXManifest.xml"
}
```

**Reset app data:**
```powershell
# Clears all app data and settings
Get-AppxPackage -Name "InstallVibe" | Reset-AppxPackage
```

---

## Troubleshooting

### Installation Fails with "Publisher not trusted"

**Symptom:** Error code 0x800B0109

**Solution:**
1. Verify certificate is installed:
   ```powershell
   Get-ChildItem Cert:\LocalMachine\Root | Where-Object {$_.Subject -like "*YourCompany*"}
   ```
2. If not found, install certificate (see [Certificate Installation](#certificate-installation))
3. Retry installation

### "Version number must be higher" Error

**Symptom:** Cannot install update or reinstall

**Solution:**
```powershell
# Uninstall current version first
Get-AppxPackage -Name "InstallVibe" | Remove-AppxPackage

# Then install new version
Add-AppxPackage -Path "InstallVibe_1.1.0.0_bundle.msixbundle"
```

### Update Not Detected

**Symptom:** App doesn't show update notification

**Check update server:**
```powershell
Invoke-WebRequest -Uri "https://internal-updates.company.com/installvibe/InstallVibe.appinstaller"
```

**Check last update check:**
1. Open InstallVibe
2. Navigate to Settings
3. Check "Last checked for updates"

**Force update check:**
- Restart application
- Or wait for configured `HoursBetweenUpdateChecks`

### Event Viewer Diagnostics

**Check deployment logs:**

1. Open **Event Viewer**
2. Navigate to:
   ```
   Applications and Services Logs
   └── Microsoft
       └── Windows
           └── AppxDeployment-Server
               └── Microsoft-Windows-AppxPackaging/Operational
   ```

**Common event IDs:**
- **501**: Deployment started
- **504**: Deployment succeeded
- **505**: Deployment failed
- **540**: Package registration
- **542**: Package removal

### Network Firewall Issues

**Allow update server through firewall:**

```powershell
# Add firewall rule for update server
New-NetFirewallRule -DisplayName "InstallVibe Updates" `
    -Direction Outbound `
    -Action Allow `
    -RemoteAddress "internal-updates.company.com"
```

---

## Security Considerations

### Code Signing Certificate Security

✅ **Do:**
- Store private keys in HSM or Azure Key Vault
- Restrict access to signing certificates
- Use separate certificates for dev/test/prod
- Monitor certificate expiration
- Have backup certificates ready

❌ **Don't:**
- Store certificates on shared drives without encryption
- Use same certificate across environments
- Share certificate passwords in email/chat
- Let certificates expire without renewal plan

### Update Server Security

**HTTPS Only:**
- Always use HTTPS for update server
- Use valid SSL certificate (not self-signed)
- Enable HSTS header

**Access Control:**
- Restrict write access to update directory
- Enable logging for all file modifications
- Use read-only permissions for IIS application pool

**Integrity Verification:**
- Sign all MSIX bundles
- Provide SHA256 checksums
- Verify downloads before deployment

### User Privacy

**Data Collection:**
- InstallVibe stores data locally in `%LOCALAPPDATA%\InstallVibe`
- No telemetry sent to external servers by default
- Database is not encrypted by default (use SQLCipher if needed)

**Network Communication:**
- Update checks only contact configured update server
- No external API calls
- Offline mode available for air-gapped environments

---

## Deployment Checklist

### Pre-Deployment

- [ ] Obtain or generate code-signing certificate
- [ ] Install certificate on test machine
- [ ] Test MSIX installation on clean Windows 10/11 VM
- [ ] Verify app launches and core functionality works
- [ ] Set up internal update server
- [ ] Configure appinstaller with internal URLs
- [ ] Test update mechanism
- [ ] Prepare documentation for end users

### Deployment

- [ ] Deploy certificate via Group Policy
- [ ] Deploy InstallVibe via chosen method (GPO/SCCM/Intune)
- [ ] Monitor deployment logs in Event Viewer
- [ ] Verify installations on sample machines
- [ ] Collect user feedback
- [ ] Document any issues encountered

### Post-Deployment

- [ ] Monitor update server logs for download activity
- [ ] Track update adoption rate
- [ ] Schedule regular updates (monthly/quarterly)
- [ ] Maintain update server (SSL cert renewal, etc.)
- [ ] Plan for certificate renewal before expiration
- [ ] Review and update deployment procedures

---

## Support Escalation

### Level 1 - End User Support

**Common issues:**
- App won't launch → Restart computer
- Missing guides → Check network connection
- Update failed → Restart app

### Level 2 - IT Help Desk

**Tools:**
- Event Viewer logs
- PowerShell verification scripts
- Network connectivity tests

**Escalate if:**
- Certificate trust issues
- Deployment failures across multiple machines
- Update server connectivity problems

### Level 3 - IT Security / Infrastructure

**Advanced troubleshooting:**
- Certificate infrastructure issues
- Group Policy conflicts
- Network/firewall restrictions
- Code signing problems

---

## Additional Resources

- [MSIX Documentation](https://docs.microsoft.com/en-us/windows/msix/)
- [App Installer File](https://docs.microsoft.com/en-us/windows/msix/app-installer/app-installer-file-overview)
- [Group Policy Deployment](https://docs.microsoft.com/en-us/windows/client-management/mdm/policy-csp-applicationmanagement)
- [Troubleshooting MSIX](https://docs.microsoft.com/en-us/windows/msix/troubleshooting)

---

**Document Version:** 1.0
**Last Updated:** 2024-01-15
**Contact:** IT Support - support@yourcompany.com
