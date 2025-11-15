<#
.SYNOPSIS
    Signs MSIX packages with a code-signing certificate

.DESCRIPTION
    This script signs MSIX packages and bundles for production deployment.
    Supports both local certificate files (.pfx) and Azure Key Vault certificates.

.PARAMETER PackagePath
    Path to the MSIX package or bundle to sign

.PARAMETER CertificatePath
    Path to the .pfx certificate file (for local signing)

.PARAMETER CertificatePassword
    Password for the .pfx certificate (use SecureString in production)

.PARAMETER CertificateThumbprint
    Thumbprint of installed certificate (alternative to file-based cert)

.PARAMETER TimestampServer
    URL of the timestamp server (default: http://timestamp.digicert.com)

.PARAMETER UseAzureKeyVault
    Switch to use Azure Key Vault for signing

.PARAMETER KeyVaultName
    Azure Key Vault name (required if UseAzureKeyVault is set)

.PARAMETER CertificateName
    Certificate name in Azure Key Vault

.EXAMPLE
    .\Sign-MSIX.ps1 -PackagePath ".\InstallVibe_1.0.0.0_bundle.msixbundle" -CertificatePath ".\cert.pfx" -CertificatePassword "MyPassword"

.EXAMPLE
    .\Sign-MSIX.ps1 -PackagePath ".\InstallVibe_1.0.0.0_bundle.msixbundle" -CertificateThumbprint "ABC123..."

.EXAMPLE
    .\Sign-MSIX.ps1 -PackagePath ".\InstallVibe_1.0.0.0_bundle.msixbundle" -UseAzureKeyVault -KeyVaultName "MyKeyVault" -CertificateName "InstallVibe-CodeSigning"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,

    [Parameter(Mandatory = $false)]
    [string]$CertificatePath,

    [Parameter(Mandatory = $false)]
    [string]$CertificatePassword,

    [Parameter(Mandatory = $false)]
    [string]$CertificateThumbprint,

    [Parameter(Mandatory = $false)]
    [string]$TimestampServer = "http://timestamp.digicert.com",

    [Parameter(Mandatory = $false)]
    [switch]$UseAzureKeyVault,

    [Parameter(Mandatory = $false)]
    [string]$KeyVaultName,

    [Parameter(Mandatory = $false)]
    [string]$CertificateName
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Check if SignTool.exe is available
$signToolPath = & "${env:ProgramFiles(x86)}\Windows Kits\10\bin\**\x64\signtool.exe" | Select-Object -First 1
if (-not $signToolPath) {
    Write-Error "SignTool.exe not found. Please install Windows SDK."
    exit 1
}

Write-Host "Using SignTool: $signToolPath" -ForegroundColor Cyan

# Verify package exists
if (-not (Test-Path $PackagePath)) {
    Write-Error "Package not found: $PackagePath"
    exit 1
}

Write-Host "Signing package: $PackagePath" -ForegroundColor Green

# Determine signing method
if ($UseAzureKeyVault) {
    #region Azure Key Vault Signing
    Write-Host "Using Azure Key Vault for signing" -ForegroundColor Cyan

    if (-not $KeyVaultName -or -not $CertificateName) {
        Write-Error "KeyVaultName and CertificateName are required for Azure Key Vault signing"
        exit 1
    }

    # Install Azure PowerShell module if not present
    if (-not (Get-Module -ListAvailable -Name Az.KeyVault)) {
        Write-Host "Installing Az.KeyVault module..." -ForegroundColor Yellow
        Install-Module -Name Az.KeyVault -Scope CurrentUser -Force
    }

    # Import module
    Import-Module Az.KeyVault

    # Login to Azure (will use existing session if available)
    try {
        $context = Get-AzContext
        if (-not $context) {
            Write-Host "Connecting to Azure..." -ForegroundColor Yellow
            Connect-AzAccount
        }
    }
    catch {
        Write-Error "Failed to connect to Azure: $_"
        exit 1
    }

    # Get certificate from Key Vault
    Write-Host "Retrieving certificate from Key Vault: $KeyVaultName/$CertificateName" -ForegroundColor Cyan
    $cert = Get-AzKeyVaultCertificate -VaultName $KeyVaultName -Name $CertificateName

    if (-not $cert) {
        Write-Error "Certificate not found in Key Vault: $CertificateName"
        exit 1
    }

    # Export certificate to temporary file
    $tempCertPath = Join-Path $env:TEMP "temp_cert.pfx"
    $secret = Get-AzKeyVaultSecret -VaultName $KeyVaultName -Name $CertificateName
    $secretValueText = $secret.SecretValue | ConvertFrom-SecureString -AsPlainText
    [System.Convert]::FromBase64String($secretValueText) | Set-Content $tempCertPath -Encoding Byte

    $CertificatePath = $tempCertPath
    #endregion
}
elseif ($CertificateThumbprint) {
    #region Certificate Store Signing
    Write-Host "Using installed certificate with thumbprint: $CertificateThumbprint" -ForegroundColor Cyan

    # Sign using thumbprint
    $signArgs = @(
        "sign",
        "/fd", "SHA256",
        "/sha1", $CertificateThumbprint,
        "/tr", $TimestampServer,
        "/td", "SHA256",
        "/v",
        $PackagePath
    )

    Write-Host "Executing: signtool.exe $($signArgs -join ' ')" -ForegroundColor Gray
    & $signToolPath $signArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Signing failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }

    Write-Host "✅ Package signed successfully!" -ForegroundColor Green
    exit 0
    #endregion
}
elseif ($CertificatePath) {
    #region PFX File Signing
    Write-Host "Using certificate file: $CertificatePath" -ForegroundColor Cyan

    if (-not (Test-Path $CertificatePath)) {
        Write-Error "Certificate file not found: $CertificatePath"
        exit 1
    }
    #endregion
}
else {
    Write-Error "Must provide either CertificatePath, CertificateThumbprint, or UseAzureKeyVault"
    exit 1
}

# Sign with PFX file
$signArgs = @(
    "sign",
    "/f", $CertificatePath,
    "/fd", "SHA256",
    "/tr", $TimestampServer,
    "/td", "SHA256",
    "/v"
)

# Add password if provided
if ($CertificatePassword) {
    $signArgs += "/p"
    $signArgs += $CertificatePassword
}

$signArgs += $PackagePath

Write-Host "Executing: signtool.exe $($signArgs -join ' ')" -ForegroundColor Gray
& $signToolPath $signArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "Signing failed with exit code $LASTEXITCODE"

    # Clean up temp cert if using Azure Key Vault
    if ($UseAzureKeyVault -and (Test-Path $tempCertPath)) {
        Remove-Item $tempCertPath -Force
    }

    exit $LASTEXITCODE
}

# Clean up temp cert if using Azure Key Vault
if ($UseAzureKeyVault -and (Test-Path $tempCertPath)) {
    Remove-Item $tempCertPath -Force
    Write-Host "Cleaned up temporary certificate file" -ForegroundColor Gray
}

Write-Host "✅ Package signed successfully!" -ForegroundColor Green

# Verify signature
Write-Host "`nVerifying signature..." -ForegroundColor Cyan
& $signToolPath verify /pa /v $PackagePath

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Signature verification passed!" -ForegroundColor Green
}
else {
    Write-Warning "Signature verification returned exit code $LASTEXITCODE"
}

exit 0
