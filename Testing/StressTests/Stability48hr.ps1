<#
.SYNOPSIS
    48-hour stability test with periodic health checks

.DESCRIPTION
    Runs InstallVibe continuously for 48 hours with automated health checks
    every 30 minutes. Monitors memory, CPU, crashes, and responsiveness.

.PARAMETER LogPath
    Path to save test logs (default: .\stability-test-log.txt)

.PARAMETER CheckIntervalMinutes
    Minutes between health checks (default: 30)

.EXAMPLE
    .\Stability48hr.ps1 -LogPath "C:\Logs\stability.txt"
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$LogPath = ".\stability-test-log.txt",

    [Parameter(Mandatory=$false)]
    [int]$CheckIntervalMinutes = 30
)

$ErrorActionPreference = "Continue"

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"

    Write-Host $logMessage -ForegroundColor $(
        switch($Level) {
            "ERROR" { "Red" }
            "WARN" { "Yellow" }
            "SUCCESS" { "Green" }
            default { "White" }
        }
    )

    Add-Content -Path $LogPath -Value $logMessage
}

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "InstallVibe 48-Hour Stability Test" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Log file: $LogPath" -ForegroundColor Gray
Write-Host "Check interval: $CheckIntervalMinutes minutes" -ForegroundColor Gray
Write-Host ""

# Initialize log file
"InstallVibe 48-Hour Stability Test" | Set-Content -Path $LogPath
"Started: $(Get-Date)" | Add-Content -Path $LogPath
"" | Add-Content -Path $LogPath

Write-Log "Starting 48-hour stability test" "INFO"

# Verify InstallVibe is running
$process = Get-Process -Name "InstallVibe" -ErrorAction SilentlyContinue
if (-not $process) {
    Write-Log "InstallVibe not running. Please start the application first." "ERROR"
    exit 1
}

$startTime = Get-Date
$endTime = $startTime.AddHours(48)
$checkNumber = 0
$totalChecks = [int](48 * 60 / $CheckIntervalMinutes)
$crashes = 0
$hangs = 0

Write-Log "Test will run until: $endTime" "INFO"
Write-Log "Expected checks: $totalChecks" "INFO"
Write-Host ""

while ((Get-Date) -lt $endTime) {
    $checkNumber++
    $elapsed = ((Get-Date) - $startTime).TotalHours
    $remaining = ($endTime - (Get-Date)).TotalHours

    Write-Log "Health Check #$checkNumber/$totalChecks (Elapsed: $([math]::Round($elapsed, 1))h, Remaining: $([math]::Round($remaining, 1))h)" "INFO"

    # Check if process is still running
    $process = Get-Process -Name "InstallVibe" -ErrorAction SilentlyContinue
    if (-not $process) {
        Write-Log "Process crashed or terminated!" "ERROR"
        $crashes++

        # Attempt to restart
        Write-Log "Attempting to restart InstallVibe..." "WARN"
        # You would add restart logic here based on your deployment
        # For MSIX: Start-Process shell:AppsFolder\[AppUserModelId]

        Start-Sleep -Seconds 30
        $process = Get-Process -Name "InstallVibe" -ErrorAction SilentlyContinue
        if ($process) {
            Write-Log "Successfully restarted" "SUCCESS"
        }
        else {
            Write-Log "Failed to restart. Exiting test." "ERROR"
            break
        }
    }

    # Memory check
    $memoryMB = [int]($process.WorkingSet64 / 1MB)
    Write-Log "Memory usage: $memoryMB MB" "INFO"

    if ($memoryMB -gt 500) {
        Write-Log "High memory usage detected!" "WARN"
    }

    # CPU check
    $cpuPercent = [int]($process.CPU)
    Write-Log "CPU time: $cpuPercent seconds" "INFO"

    # Thread count
    $threadCount = $process.Threads.Count
    Write-Log "Thread count: $threadCount" "INFO"

    if ($threadCount -gt 100) {
        Write-Log "High thread count detected!" "WARN"
    }

    # Handle count
    $handleCount = $process.HandleCount
    Write-Log "Handle count: $handleCount" "INFO"

    if ($handleCount -gt 5000) {
        Write-Log "High handle count detected!" "WARN"
    }

    # Responsiveness check
    try {
        if (-not $process.Responding) {
            Write-Log "Application not responding!" "ERROR"
            $hangs++
        }
        else {
            Write-Log "Application responsive: OK" "SUCCESS"
        }
    }
    catch {
        Write-Log "Error checking responsiveness: $_" "ERROR"
    }

    # Collect performance counter snapshot
    $perfData = @{
        Time = Get-Date
        Memory = $memoryMB
        Threads = $threadCount
        Handles = $handleCount
        CPU = $cpuPercent
    }

    # Export to CSV for analysis
    $csvPath = $LogPath -replace '\.txt$', '.csv'
    if (-not (Test-Path $csvPath)) {
        "Time,Memory_MB,Threads,Handles,CPU_Seconds" | Set-Content -Path $csvPath
    }
    "$($perfData.Time),$($perfData.Memory),$($perfData.Threads),$($perfData.Handles),$($perfData.CPU)" |
        Add-Content -Path $csvPath

    Write-Host ""

    # Wait for next check interval
    $sleepSeconds = $CheckIntervalMinutes * 60
    Write-Log "Sleeping for $CheckIntervalMinutes minutes..." "INFO"
    Start-Sleep -Seconds $sleepSeconds
}

$actualEndTime = Get-Date
$totalDuration = ($actualEndTime - $startTime).TotalHours

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "48-Hour Stability Test Complete!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan

Write-Log "Test completed after $([math]::Round($totalDuration, 2)) hours" "SUCCESS"
Write-Log "Total health checks: $checkNumber" "INFO"
Write-Log "Crashes: $crashes" $(if($crashes -eq 0){"SUCCESS"}else{"ERROR"})
Write-Log "Hangs: $hangs" $(if($hangs -eq 0){"SUCCESS"}else{"WARN"})

# Final process check
$finalProcess = Get-Process -Name "InstallVibe" -ErrorAction SilentlyContinue
if ($finalProcess) {
    $finalMemory = [int]($finalProcess.WorkingSet64 / 1MB)
    Write-Log "Final memory usage: $finalMemory MB" "INFO"
}

Write-Host ""
Write-Host "Logs saved to: $LogPath" -ForegroundColor Cyan
Write-Host "Performance data: $csvPath" -ForegroundColor Cyan
Write-Host ""

if ($crashes -eq 0 -and $hangs -eq 0) {
    Write-Host "✓ Test PASSED - No crashes or hangs detected" -ForegroundColor Green
}
else {
    Write-Host "✗ Test FAILED - $crashes crashes, $hangs hangs" -ForegroundColor Red
}
