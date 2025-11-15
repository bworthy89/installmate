# Code Signing Guide for InstallVibe

Complete guide for signing MSIX packages for production deployment.

## Table of Contents

1. [Certificate Requirements](#certificate-requirements)
2. [Local Development Signing](#local-development-signing)
3. [Production Signing](#production-signing)
4. [Azure Key Vault Integration](#azure-key-vault-integration)
5. [CI/CD Integration](#cicd-integration)
6. [Certificate Management](#certificate-management)
7. [Troubleshooting](#troubleshooting)

---

## Certificate Requirements

### Development Environment

For local testing, use a self-signed certificate:

```powershell
# Generate self-signed certificate
New-SelfSignedCertificate -Type Custom -Subject "CN=Your Company" `
    -KeyUsage DigitalSignature -FriendlyName "InstallVibe Dev Certificate" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

# Export to PFX
$cert = Get-ChildItem -Path Cert:\CurrentUser\My | Where-Object {$_.Subject -eq "CN=Your Company"}
$password = ConvertTo-SecureString -String "DevPassword123" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath ".\Dev_Certificate.pfx" -Password $password
```

**Trust the certificate:**
```powershell
Import-PfxCertificate -FilePath ".\Dev_Certificate.pfx" `
    -CertStoreLocation Cert:\LocalMachine\Root `
    -Password (ConvertTo-SecureString -String "DevPassword123" -Force -AsPlainText)
```

### Production Environment

Production requires a certificate from a trusted Certificate Authority (CA):

**Options:**
1. **Public CA**: DigiCert, GlobalSign, Sectigo (for public distribution)
2. **Enterprise CA**: Your organization's internal CA (for internal deployment)
3. **EV Code Signing**: Extended Validation (highest trust level, no SmartScreen warnings)

**Certificate must include:**
- Code Signing usage (`1.3.6.1.5.5.7.3.3`)
- Subject CN matching `Publisher` in Package.appxmanifest
- Valid for at least 1 year
- 2048-bit RSA or 256-bit ECC minimum

---

## Local Development Signing

### Using Visual Studio

1. Right-click `InstallVibe.Package` project
2. Select **Properties** → **Packaging**
3. Click **Choose Certificate...**
4. Select **Create test certificate...** or **Select from file...**

### Using PowerShell Script

```powershell
# Sign with PFX file
.\Scripts\Sign-MSIX.ps1 `
    -PackagePath ".\InstallVibe_1.0.0.0_bundle.msixbundle" `
    -CertificatePath ".\Dev_Certificate.pfx" `
    -CertificatePassword "DevPassword123"
```

### Using SignTool Directly

```cmd
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe" sign ^
    /f Dev_Certificate.pfx ^
    /p DevPassword123 ^
    /fd SHA256 ^
    /tr http://timestamp.digicert.com ^
    /td SHA256 ^
    /v ^
    InstallVibe_1.0.0.0_bundle.msixbundle
```

---

## Production Signing

### Option 1: File-Based Certificate

**Secure storage:**
- Never commit certificate files to source control
- Store in secure location (password manager, encrypted drive)
- Use environment variables for passwords in CI/CD

```powershell
# Production signing example
.\Scripts\Sign-MSIX.ps1 `
    -PackagePath ".\AppPackages\InstallVibe_1.0.0.0_bundle.msixbundle" `
    -CertificatePath "$env:CERT_PATH\ProductionCert.pfx" `
    -CertificatePassword "$env:CERT_PASSWORD"
```

### Option 2: Certificate Store

**Install certificate to store:**
```powershell
Import-PfxCertificate -FilePath "ProductionCert.pfx" `
    -CertStoreLocation Cert:\CurrentUser\My `
    -Password (Read-Host "Enter certificate password" -AsSecureString)
```

**Sign using thumbprint:**
```powershell
# Get certificate thumbprint
$cert = Get-ChildItem -Path Cert:\CurrentUser\My | Where-Object {$_.Subject -like "*Your Company*"}
$thumbprint = $cert.Thumbprint

# Sign
.\Scripts\Sign-MSIX.ps1 `
    -PackagePath ".\InstallVibe_1.0.0.0_bundle.msixbundle" `
    -CertificateThumbprint $thumbprint
```

---

## Azure Key Vault Integration

### Setup

1. **Create Azure Key Vault:**
   ```bash
   az keyvault create --name InstallVibeKeyVault \
       --resource-group InstallVibeRG \
       --location eastus
   ```

2. **Import Certificate:**
   ```bash
   az keyvault certificate import --vault-name InstallVibeKeyVault \
       --name InstallVibe-CodeSigning \
       --file ProductionCert.pfx \
       --password "YourCertPassword"
   ```

3. **Grant Access:**
   ```bash
   # For service principal (CI/CD)
   az keyvault set-policy --name InstallVibeKeyVault \
       --spn YOUR_APP_ID \
       --certificate-permissions get \
       --secret-permissions get
   ```

### Sign Using Key Vault

```powershell
.\Scripts\Sign-MSIX.ps1 `
    -PackagePath ".\InstallVibe_1.0.0.0_bundle.msixbundle" `
    -UseAzureKeyVault `
    -KeyVaultName "InstallVibeKeyVault" `
    -CertificateName "InstallVibe-CodeSigning"
```

---

## CI/CD Integration

### GitHub Actions

```yaml
- name: Sign MSIX Package
  shell: pwsh
  env:
    CERT_PASSWORD: ${{ secrets.CERTIFICATE_PASSWORD }}
  run: |
    # Decode base64 certificate from GitHub secret
    $certBytes = [Convert]::FromBase64String("${{ secrets.CERTIFICATE_BASE64 }}")
    $certPath = Join-Path $env:TEMP "signing_cert.pfx"
    [IO.File]::WriteAllBytes($certPath, $certBytes)

    # Sign package
    .\Scripts\Sign-MSIX.ps1 `
        -PackagePath ".\AppPackages\InstallVibe_1.0.0.0_bundle.msixbundle" `
        -CertificatePath $certPath `
        -CertificatePassword $env:CERT_PASSWORD

    # Clean up
    Remove-Item $certPath -Force
```

### Azure DevOps

```yaml
- task: PowerShell@2
  displayName: 'Sign MSIX Package'
  inputs:
    filePath: 'Scripts/Sign-MSIX.ps1'
    arguments: >
      -PackagePath "$(Build.ArtifactStagingDirectory)/InstallVibe_1.0.0.0_bundle.msixbundle"
      -UseAzureKeyVault
      -KeyVaultName "InstallVibeKeyVault"
      -CertificateName "InstallVibe-CodeSigning"
  env:
    AZURE_SUBSCRIPTION: $(AzureSubscription)
```

---

## Certificate Management

### Certificate Rotation

When certificate expires or needs renewal:

1. **Obtain new certificate** from CA
2. **Update Azure Key Vault** (if using):
   ```bash
   az keyvault certificate import --vault-name InstallVibeKeyVault \
       --name InstallVibe-CodeSigning \
       --file NewCert.pfx \
       --password "NewPassword"
   ```
3. **Update Package.appxmanifest** Publisher if CN changed
4. **Rebuild and sign** all packages
5. **Deploy new certificate** to client machines (if enterprise CA)

### Certificate Backup

**Backup production certificates:**
```powershell
# Export certificate (private key requires password)
$cert = Get-ChildItem Cert:\CurrentUser\My\THUMBPRINT
Export-PfxCertificate -Cert $cert -FilePath "Backup_Cert.pfx" `
    -Password (Read-Host "Enter password" -AsSecureString)

# Store securely:
# - Password manager
# - Encrypted USB drive
# - Secure file share with access logging
```

### Timestamp Servers

Always use timestamping to ensure packages remain valid after certificate expires:

**Recommended timestamp servers:**
- DigiCert: `http://timestamp.digicert.com`
- GlobalSign: `http://timestamp.globalsign.com/scripts/timstamp.dll`
- Sectigo: `http://timestamp.sectigo.com`
- Entrust: `http://timestamp.entrust.net/TSS/RFC3161sha2TS`

**Why timestamp?**
- Packages signed with timestamp remain valid after cert expires
- Proves package was signed when certificate was valid
- Critical for long-term package validity

---

## Troubleshooting

### Error: "SignTool Error: No certificates were found that met all the given criteria"

**Cause:** Certificate not found or doesn't match criteria.

**Solution:**
1. Verify certificate is installed: `Get-ChildItem Cert:\CurrentUser\My`
2. Check certificate has Code Signing usage
3. Ensure certificate is not expired

### Error: "The Publisher attribute must match the publisher subject"

**Cause:** Certificate CN doesn't match Package.appxmanifest Publisher.

**Solution:**
Update Package.appxmanifest:
```xml
<Identity Publisher="CN=Your Exact Certificate Subject" />
```

Get exact subject from certificate:
```powershell
$cert = Get-ChildItem Cert:\CurrentUser\My\THUMBPRINT
$cert.Subject
```

### Error: "SignTool Error: WinVerifyTrust returned error: 0x800B0109"

**Cause:** Certificate not trusted (missing root CA).

**Solution:**
Install root CA certificate:
```powershell
# For enterprise CA
Import-Certificate -FilePath "RootCA.cer" -CertStoreLocation Cert:\LocalMachine\Root

# Or via Group Policy for domain computers
```

### Warning: "SmartScreen" warnings after installation

**Cause:** Certificate doesn't have enough reputation or is not EV.

**Solutions:**
1. **Wait for reputation**: Microsoft SmartScreen builds reputation over time
2. **Use EV Certificate**: Extended Validation certificates bypass SmartScreen
3. **Submit to Microsoft**: Request SmartScreen whitelisting (for high-volume apps)

### Timestamp Server Timeout

**Cause:** Timestamp server unreachable or slow.

**Solution:**
1. Try different timestamp server
2. Increase timeout (not directly supported, retry logic in script)
3. Check network/firewall allows HTTP to timestamp server

---

## Best Practices

✅ **Do:**
- Always use timestamping
- Store certificates in Azure Key Vault for CI/CD
- Use SHA256 for signing and timestamping
- Rotate certificates before expiration
- Test signed packages on clean Windows install
- Log all signing operations

❌ **Don't:**
- Commit certificate files (.pfx) to source control
- Use self-signed certificates in production
- Share certificate passwords via email/chat
- Skip certificate backups
- Use MD5 or SHA1 (deprecated)

---

## Additional Resources

- [Microsoft: Sign MSIX packages](https://docs.microsoft.com/en-us/windows/msix/package/sign-app-package-using-signtool)
- [SignTool documentation](https://docs.microsoft.com/en-us/windows/win32/seccrypto/signtool)
- [Azure Key Vault certificates](https://docs.microsoft.com/en-us/azure/key-vault/certificates/about-certificates)
