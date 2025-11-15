# InstallVibe MSIX Deployment Instructions

Complete guide for IT departments to deploy and manage InstallVibe updates.

## Table of Contents

1. [Hosting Options](#hosting-options)
2. [File Upload Structure](#file-upload-structure)
3. [Azure Blob Storage Deployment](#azure-blob-storage-deployment)
4. [AWS S3 Deployment](#aws-s3-deployment)
5. [Internal Web Server Deployment](#internal-web-server-deployment)
6. [Staged Rollout Strategy](#staged-rollout-strategy)
7. [Update Configuration Examples](#update-configuration-examples)
8. [Testing Updates](#testing-updates)
9. [Troubleshooting](#troubleshooting)

---

## Hosting Options

InstallVibe updates can be hosted on:

| Option | Pros | Cons | Best For |
|--------|------|------|----------|
| **Azure Blob Storage** | Highly available, CDN integration, cost-effective | Requires Azure subscription | Cloud-first organizations |
| **AWS S3** | Global distribution, mature platform | Requires AWS account | AWS-centric organizations |
| **Internal Web Server** | Full control, air-gapped option | Requires IT maintenance | Offline/secure environments |
| **SharePoint** | Existing infrastructure | Performance limitations | Small deployments (<100 users) |

---

## File Upload Structure

Organize files on your hosting server as follows:

```
/installvibe/
├── InstallVibe.appinstaller                              # Main installer file (users install from this)
├── InstallVibe_1.0.0.0_x64_arm64_bundle.msixbundle      # Current production release
├── InstallVibe_1.0.1.0_x64_arm64_bundle.msixbundle      # Next release (staged)
├── releases/
│   ├── 1.0.0.0/
│   │   ├── InstallVibe_1.0.0.0_x64_arm64_bundle.msixbundle
│   │   ├── InstallVibe_1.0.0.0_x64.msix
│   │   ├── InstallVibe_1.0.0.0_arm64.msix
│   │   └── release-notes.md
│   └── 1.0.1.0/
│       ├── InstallVibe_1.0.1.0_x64_arm64_bundle.msixbundle
│       ├── InstallVibe_1.0.1.0_x64.msix
│       ├── InstallVibe_1.0.1.0_arm64.msix
│       └── release-notes.md
├── dependencies/
│   ├── Microsoft.VCLibs.x64.14.00.Desktop.appx
│   └── Microsoft.VCLibs.arm64.14.00.Desktop.appx
└── channels/
    ├── InstallVibe-dev.appinstaller                      # Development channel (early access)
    ├── InstallVibe-testing.appinstaller                  # Testing channel (QA/UAT)
    └── InstallVibe-production.appinstaller               # Production channel (stable)
```

---

## Azure Blob Storage Deployment

### Prerequisites

- Azure subscription
- Azure Storage Account
- Azure CLI or Azure Portal access

### Step 1: Create Storage Account

```bash
# Azure CLI
az group create --name InstallVibeRG --location eastus

az storage account create \
    --name installvibeupdates \
    --resource-group InstallVibeRG \
    --location eastus \
    --sku Standard_LRS \
    --kind StorageV2
```

### Step 2: Create Container

```bash
az storage container create \
    --name installvibe \
    --account-name installvibeupdates \
    --public-access blob
```

### Step 3: Upload Files

```bash
# Upload MSIX bundle
az storage blob upload \
    --container-name installvibe \
    --file "InstallVibe_1.0.0.0_x64_arm64_bundle.msixbundle" \
    --name "InstallVibe_1.0.0.0_x64_arm64_bundle.msixbundle" \
    --account-name installvibeupdates

# Upload App Installer
az storage blob upload \
    --container-name installvibe \
    --file "InstallVibe.appinstaller" \
    --name "InstallVibe.appinstaller" \
    --account-name installvibeupdates \
    --content-type "application/xml"
```

### Step 4: Enable CDN (Optional)

```bash
az cdn profile create \
    --resource-group InstallVibeRG \
    --name InstallVibeCDN \
    --sku Standard_Microsoft

az cdn endpoint create \
    --resource-group InstallVibeRG \
    --profile-name InstallVibeCDN \
    --name installvibe-updates \
    --origin installvibeupdates.blob.core.windows.net \
    --origin-host-header installvibeupdates.blob.core.windows.net
```

### Step 5: Update Installer URL

Your installer URL will be:
```
https://installvibeupdates.blob.core.windows.net/installvibe/InstallVibe.appinstaller

# Or with CDN:
https://installvibe-updates.azureedge.net/installvibe/InstallVibe.appinstaller
```

Update `InstallVibe.appinstaller` Uri attributes to match.

---

## AWS S3 Deployment

### Step 1: Create S3 Bucket

```bash
# AWS CLI
aws s3 mb s3://installvibe-updates --region us-east-1

# Configure bucket for static website hosting
aws s3 website s3://installvibe-updates \
    --index-document index.html
```

### Step 2: Upload Files

```bash
# Upload MSIX bundle
aws s3 cp InstallVibe_1.0.0.0_x64_arm64_bundle.msixbundle \
    s3://installvibe-updates/installvibe/ \
    --acl public-read

# Upload App Installer
aws s3 cp InstallVibe.appinstaller \
    s3://installvibe-updates/installvibe/ \
    --acl public-read \
    --content-type "application/xml"
```

### Step 3: Configure CloudFront CDN (Optional)

```bash
aws cloudfront create-distribution \
    --origin-domain-name installvibe-updates.s3.amazonaws.com \
    --default-root-object index.html
```

### Step 4: Set Bucket Policy

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "PublicReadGetObject",
      "Effect": "Allow",
      "Principal": "*",
      "Action": "s3:GetObject",
      "Resource": "arn:aws:s3:::installvibe-updates/installvibe/*"
    }
  ]
}
```

---

## Internal Web Server Deployment

### IIS (Windows Server)

1. **Create Web Directory**
   ```powershell
   New-Item -Path "C:\inetpub\wwwroot\installvibe" -ItemType Directory
   ```

2. **Copy Files**
   ```powershell
   Copy-Item "InstallVibe_1.0.0.0_x64_arm64_bundle.msixbundle" "C:\inetpub\wwwroot\installvibe\"
   Copy-Item "InstallVibe.appinstaller" "C:\inetpub\wwwroot\installvibe\"
   ```

3. **Configure MIME Types**
   ```powershell
   # IIS must serve .appinstaller files with correct content type
   Add-WebConfigurationProperty -PSPath "IIS:\Sites\Default Web Site\installvibe" `
       -Filter "system.webServer/staticContent" `
       -Name "." `
       -Value @{fileExtension='.appinstaller'; mimeType='application/xml'}

   Add-WebConfigurationProperty -PSPath "IIS:\Sites\Default Web Site\installvibe" `
       -Filter "system.webServer/staticContent" `
       -Name "." `
       -Value @{fileExtension='.msixbundle'; mimeType='application/octet-stream'}
   ```

4. **Enable Directory Browsing (Optional)**
   ```powershell
   Set-WebConfigurationProperty -Filter /system.webServer/directoryBrowse `
       -Name enabled -Value $true `
       -PSPath "IIS:\Sites\Default Web Site\installvibe"
   ```

### Apache/Nginx

Add MIME types to configuration:

**Apache (.htaccess or httpd.conf):**
```apache
AddType application/xml .appinstaller
AddType application/octet-stream .msix
AddType application/octet-stream .msixbundle
```

**Nginx (nginx.conf):**
```nginx
types {
    application/xml                             appinstaller;
    application/octet-stream                    msix msixbundle;
}
```

---

## Staged Rollout Strategy

Deploy updates gradually to minimize risk:

### Channel-Based Rollout

Create separate appinstaller files for each channel:

**1. Development Channel (10% - Early Adopters)**
```xml
<!-- InstallVibe-dev.appinstaller -->
<AppInstaller Version="1.1.0.0" Uri="https://updates.company.com/installvibe/channels/InstallVibe-dev.appinstaller">
  <MainBundle Version="1.1.0.0" Uri="https://updates.company.com/installvibe/InstallVibe_1.1.0.0_bundle.msixbundle" />
  <UpdateSettings>
    <OnLaunch HoursBetweenUpdateChecks="0" ShowPrompt="true" UpdateBlocksActivation="false" />
  </UpdateSettings>
</AppInstaller>
```

**2. Testing Channel (25% - QA/UAT)**
```xml
<!-- InstallVibe-testing.appinstaller -->
<AppInstaller Version="1.0.5.0" Uri="https://updates.company.com/installvibe/channels/InstallVibe-testing.appinstaller">
  <MainBundle Version="1.0.5.0" Uri="https://updates.company.com/installvibe/InstallVibe_1.0.5.0_bundle.msixbundle" />
  <UpdateSettings>
    <OnLaunch HoursBetweenUpdateChecks="12" ShowPrompt="true" UpdateBlocksActivation="false" />
  </UpdateSettings>
</AppInstaller>
```

**3. Production Channel (100% - All Users)**
```xml
<!-- InstallVibe-production.appinstaller (or InstallVibe.appinstaller) -->
<AppInstaller Version="1.0.0.0" Uri="https://updates.company.com/installvibe/InstallVibe.appinstaller">
  <MainBundle Version="1.0.0.0" Uri="https://updates.company.com/installvibe/InstallVibe_1.0.0.0_bundle.msixbundle" />
  <UpdateSettings>
    <OnLaunch HoursBetweenUpdateChecks="24" ShowPrompt="true" UpdateBlocksActivation="false" />
  </UpdateSettings>
</AppInstaller>
```

### Rollout Timeline Example

| Day | Channel | Version | Users |
|-----|---------|---------|-------|
| 1-3 | Development | 1.1.0 | 10 users (early adopters) |
| 4-7 | Testing | 1.1.0 | 50 users (QA + power users) |
| 8-14 | Production | 1.1.0 | All users (500+) |

---

## Update Configuration Examples

### Aggressive Updates (Critical Security Patch)

```xml
<UpdateSettings>
  <!-- Check on every launch -->
  <OnLaunch HoursBetweenUpdateChecks="0" ShowPrompt="true" UpdateBlocksActivation="true" />
  <!-- Force update immediately -->
  <ForceUpdateFromAnyVersion>true</ForceUpdateFromAnyVersion>
</UpdateSettings>
```

### Standard Updates (Regular Feature Release)

```xml
<UpdateSettings>
  <!-- Check daily -->
  <OnLaunch HoursBetweenUpdateChecks="24" ShowPrompt="true" UpdateBlocksActivation="false" />
</UpdateSettings>
```

### Conservative Updates (Stable Baseline)

```xml
<UpdateSettings>
  <!-- Check weekly -->
  <OnLaunch HoursBetweenUpdateChecks="168" ShowPrompt="true" UpdateBlocksActivation="false" />
</UpdateSettings>
```

### Silent Background Updates

```xml
<UpdateSettings>
  <OnLaunch HoursBetweenUpdateChecks="24" ShowPrompt="false" UpdateBlocksActivation="false" />
  <AutomaticBackgroundTask />
</UpdateSettings>
```

---

## Testing Updates

### Test Update Flow Locally

1. **Install Initial Version**
   ```
   Add-AppxPackage InstallVibe_1.0.0.0_bundle.msixbundle
   ```

2. **Host Update Files Locally**
   ```powershell
   # Simple Python web server
   cd C:\InstallVibeUpdates
   python -m http.server 8000
   ```

3. **Update appinstaller to Point to Localhost**
   ```xml
   <AppInstaller Uri="http://localhost:8000/InstallVibe.appinstaller">
     <MainBundle Uri="http://localhost:8000/InstallVibe_1.0.1.0_bundle.msixbundle" Version="1.0.1.0" />
   </AppInstaller>
   ```

4. **Launch App and Verify Update Prompt**

5. **Check Event Viewer for Errors**
   - Event Viewer → Applications and Services Logs → Microsoft → Windows → AppxDeployment-Server

---

## Troubleshooting

### Update Not Detected

**Symptom**: App doesn't prompt for update.

**Solutions**:
1. Verify `HoursBetweenUpdateChecks` has passed since last check
2. Check network connectivity to update server
3. Verify appinstaller URL is accessible from client machine:
   ```powershell
   Invoke-WebRequest -Uri "https://updates.company.com/installvibe/InstallVibe.appinstaller"
   ```

### Version Number Error

**Symptom**: "The app package's version is not correct"

**Solutions**:
1. Ensure new version > installed version
2. Version format must be `Major.Minor.Build.Revision` (all integers)
3. Check both `appinstaller` and `appxmanifest` versions match

### Certificate Trust Issues

**Symptom**: "The app package's publisher is not trusted"

**Solutions**:
1. Install signing certificate to Trusted Root:
   ```powershell
   Import-Certificate -FilePath "YourCert.cer" -CertStoreLocation Cert:\LocalMachine\Root
   ```
2. Deploy via Group Policy for domain computers

### Network/Firewall Blocking

**Symptom**: Update download fails or times out.

**Solutions**:
1. Whitelist update server domain in firewall
2. For internal servers, ensure DNS resolves correctly
3. Test with `curl` or `wget` from client machine

### Forced Update Not Working

**Symptom**: `<ForceUpdateFromAnyVersion>` ignored.

**Solutions**:
1. Verify Windows version supports forced updates (Windows 10 1809+)
2. Ensure user has sufficient privileges
3. Check Event Viewer for detailed error logs

---

## Support and Escalation

For deployment issues:
1. Check Event Viewer: `Applications and Services Logs → Microsoft → Windows → AppxDeployment-Server`
2. Review IIS/web server logs for 404/403 errors
3. Contact IT support with:
   - Current installed version (`Get-AppxPackage InstallVibe`)
   - Appinstaller URL being used
   - Relevant Event Viewer logs
   - Network trace (Fiddler/Wireshark) if needed

---

## Next Steps

After successful deployment:
- [ ] Monitor update adoption rate
- [ ] Review Event Viewer for deployment errors
- [ ] Collect user feedback on update experience
- [ ] Plan staged rollout for next release
- [ ] Update documentation with lessons learned
