<#
.SYNOPSIS
    Stress test: Navigate through a guide 100 times

.DESCRIPTION
    This script automates repeated navigation through a guide to test
    for memory leaks, performance degradation, and UI stability.

.PARAMETER GuideId
    ID of the guide to loop through

.PARAMETER Iterations
    Number of times to loop through the guide (default: 100)

.EXAMPLE
    .\RepeatedStepLoop.ps1 -GuideId 1 -Iterations 100
#>

param(
    [Parameter(Mandatory=$true)]
    [int]$GuideId,

    [Parameter(Mandatory=$false)]
    [int]$Iterations = 100
)

$ErrorActionPreference = "Stop"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "InstallVibe Stress Test: Repeated Navigation" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Guide ID: $GuideId" -ForegroundColor Yellow
Write-Host "Iterations: $Iterations" -ForegroundColor Yellow
Write-Host ""

# Check if UI Automation module is available
if (-not (Get-Command "Get-UIAWindow" -ErrorAction SilentlyContinue)) {
    Write-Host "Installing UIAutomation module..." -ForegroundColor Yellow
    Install-Module -Name UIAutomation -Force -Scope CurrentUser
    Import-Module UIAutomation
}

# Find InstallVibe window
$window = Get-UIAWindow -Name "InstallVibe*" -ErrorAction SilentlyContinue
if (-not $window) {
    Write-Error "InstallVibe window not found. Please start the application first."
    exit 1
}

Write-Host "Found InstallVibe window" -ForegroundColor Green
Write-Host ""

$startTime = Get-Date
$memorySnapshots = @()

for ($i = 1; $i -le $Iterations; $i++) {
    Write-Host "Iteration $i/$Iterations" -ForegroundColor Cyan

    try {
        # Navigate to guide library
        $libraryButton = $window | Get-UIAButton -AutomationId "LibraryNavButton" -ErrorAction Stop
        $libraryButton.Click()
        Start-Sleep -Milliseconds 500

        # Select the guide
        $guideList = $window | Get-UIAList -AutomationId "GuideListView" -ErrorAction Stop
        $guideItem = $guideList | Get-UIAListItem -Index 0 -ErrorAction Stop
        $guideItem.Click()
        Start-Sleep -Milliseconds 500

        # Get step count
        $stepCountText = $window | Get-UIAText -AutomationId "StepCountTextBlock" -ErrorAction SilentlyContinue
        if ($stepCountText) {
            $totalSteps = [int]($stepCountText.Name -replace '\D', '')
        }
        else {
            $totalSteps = 10 # Default assumption
        }

        # Navigate through all steps
        for ($step = 1; $step -lt $totalSteps; $step++) {
            $nextButton = $window | Get-UIAButton -AutomationId "NextStepButton" -ErrorAction SilentlyContinue
            if ($nextButton -and $nextButton.Enabled) {
                $nextButton.Click()
                Start-Sleep -Milliseconds 300
            }
            else {
                break
            }
        }

        # Capture memory usage every 10 iterations
        if ($i % 10 -eq 0) {
            $process = Get-Process -Name "InstallVibe" -ErrorAction SilentlyContinue
            if ($process) {
                $memoryMB = [int]($process.WorkingSet64 / 1MB)
                $memorySnapshots += $memoryMB
                Write-Host "  Memory: $memoryMB MB" -ForegroundColor Gray

                # Check for memory leak (> 500MB is concerning)
                if ($memoryMB -gt 500) {
                    Write-Warning "High memory usage detected: $memoryMB MB"
                }
            }
        }
    }
    catch {
        Write-Warning "Error during iteration $i: $_"
        Start-Sleep -Seconds 2
        continue
    }
}

$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Stress Test Complete!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Iterations: $Iterations" -ForegroundColor Yellow
Write-Host "Duration: $([int]$duration) seconds" -ForegroundColor Yellow
Write-Host "Avg time per iteration: $([math]::Round($duration/$Iterations, 2)) seconds" -ForegroundColor Yellow
Write-Host ""

if ($memorySnapshots.Count -gt 0) {
    $avgMemory = ($memorySnapshots | Measure-Object -Average).Average
    $maxMemory = ($memorySnapshots | Measure-Object -Maximum).Maximum
    $minMemory = ($memorySnapshots | Measure-Object -Minimum).Minimum

    Write-Host "Memory Usage:" -ForegroundColor Cyan
    Write-Host "  Min: $([int]$minMemory) MB" -ForegroundColor Gray
    Write-Host "  Max: $([int]$maxMemory) MB" -ForegroundColor Gray
    Write-Host "  Avg: $([int]$avgMemory) MB" -ForegroundColor Gray

    # Detect potential memory leak
    if ($maxMemory - $minMemory -gt 100) {
        Write-Warning "Potential memory leak detected: $([int]($maxMemory - $minMemory)) MB growth"
    }
    else {
        Write-Host "  ✓ No memory leak detected" -ForegroundColor Green
    }
}

Write-Host ""
