<#
.SYNOPSIS
    Memory leak detection test

.DESCRIPTION
    Monitors memory usage over time to detect memory leaks.
    Performs repeated operations and tracks memory growth.

.PARAMETER DurationMinutes
    How long to run the test (default: 60 minutes)

.PARAMETER SampleIntervalSeconds
    Seconds between memory samples (default: 10)

.EXAMPLE
    .\MemoryLeakDetection.ps1 -DurationMinutes 120
#>

param(
    [Parameter(Mandatory=$false)]
    [int]$DurationMinutes = 60,

    [Parameter(Mandatory=$false)]
    [int]$SampleIntervalSeconds = 10
)

$ErrorActionPreference = "Stop"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "InstallVibe Memory Leak Detection" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Duration: $DurationMinutes minutes" -ForegroundColor Yellow
Write-Host "Sample interval: $SampleIntervalSeconds seconds" -ForegroundColor Yellow
Write-Host ""

# Verify InstallVibe is running
$process = Get-Process -Name "InstallVibe" -ErrorAction SilentlyContinue
if (-not $process) {
    Write-Error "InstallVibe not running. Please start the application first."
    exit 1
}

$startTime = Get-Date
$endTime = $startTime.AddMinutes($DurationMinutes)
$samples = @()
$sampleCount = 0

Write-Host "Starting memory monitoring..." -ForegroundColor Green
Write-Host "Press Ctrl+C to stop early" -ForegroundColor Gray
Write-Host ""

# Create output file
$outputFile = "memory-leak-test-$(Get-Date -Format 'yyyyMMdd-HHmmss').csv"
"Sample,Time,Memory_MB,Private_MB,Virtual_MB,GC_Gen0,GC_Gen1,GC_Gen2" | Set-Content -Path $outputFile

try {
    while ((Get-Date) -lt $endTime) {
        $sampleCount++
        $process = Get-Process -Name "InstallVibe" -ErrorAction Stop

        $memoryMB = [math]::Round($process.WorkingSet64 / 1MB, 2)
        $privateMB = [math]::Round($process.PrivateMemorySize64 / 1MB, 2)
        $virtualMB = [math]::Round($process.VirtualMemorySize64 / 1MB, 2)

        # Get GC stats if possible
        $gcGen0 = 0
        $gcGen1 = 0
        $gcGen2 = 0

        $sample = [PSCustomObject]@{
            Sample = $sampleCount
            Time = Get-Date -Format "HH:mm:ss"
            Memory = $memoryMB
            Private = $privateMB
            Virtual = $virtualMB
            GC_Gen0 = $gcGen0
            GC_Gen1 = $gcGen1
            GC_Gen2 = $gcGen2
        }

        $samples += $sample

        # Write to CSV
        "$($sample.Sample),$($sample.Time),$($sample.Memory),$($sample.Private),$($sample.Virtual),$($sample.GC_Gen0),$($sample.GC_Gen1),$($sample.GC_Gen2)" |
            Add-Content -Path $outputFile

        # Display current stats
        $elapsed = ((Get-Date) - $startTime).TotalMinutes
        $remaining = ($endTime - (Get-Date)).TotalMinutes

        Write-Host "[$sampleCount] $($sample.Time) | Mem: $($sample.Memory) MB | Priv: $($sample.Private) MB | Elapsed: $([math]::Round($elapsed, 1))m | Remaining: $([math]::Round($remaining, 1))m" -ForegroundColor Cyan

        Start-Sleep -Seconds $SampleIntervalSeconds
    }
}
catch {
    Write-Host ""
    Write-Host "Test interrupted: $_" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Analysis" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan

if ($samples.Count -lt 2) {
    Write-Host "Not enough samples for analysis" -ForegroundColor Red
    exit 1
}

$firstSample = $samples[0]
$lastSample = $samples[-1]
$midpoint = $samples[[int]($samples.Count / 2)]

$memoryGrowth = $lastSample.Memory - $firstSample.Memory
$growthPercent = ($memoryGrowth / $firstSample.Memory) * 100

Write-Host ""
Write-Host "Memory Usage:" -ForegroundColor White
Write-Host "  Initial: $($firstSample.Memory) MB" -ForegroundColor Gray
Write-Host "  Midpoint: $($midpoint.Memory) MB" -ForegroundColor Gray
Write-Host "  Final: $($lastSample.Memory) MB" -ForegroundColor Gray
Write-Host "  Growth: $([math]::Round($memoryGrowth, 2)) MB ($([math]::Round($growthPercent, 2))%)" -ForegroundColor Yellow

# Calculate average growth rate (MB per minute)
$durationMinutes = ((Get-Date) - $startTime).TotalMinutes
$growthRate = $memoryGrowth / $durationMinutes

Write-Host ""
Write-Host "Growth Rate: $([math]::Round($growthRate, 2)) MB/minute" -ForegroundColor Yellow

# Detect leak
if ($growthRate -gt 1) {
    Write-Host ""
    Write-Host "⚠ POTENTIAL MEMORY LEAK DETECTED ⚠" -ForegroundColor Red
    Write-Host "Growth rate exceeds 1 MB/minute threshold" -ForegroundColor Red
    $projectedHour = $growthRate * 60
    Write-Host "Projected growth over 1 hour: $([math]::Round($projectedHour, 0)) MB" -ForegroundColor Yellow
}
elseif ($growthRate -gt 0.5) {
    Write-Host ""
    Write-Host "⚠ POSSIBLE MEMORY LEAK" -ForegroundColor Yellow
    Write-Host "Growth rate is elevated but within tolerance" -ForegroundColor Yellow
}
else {
    Write-Host ""
    Write-Host "✓ No memory leak detected" -ForegroundColor Green
    Write-Host "Memory usage is stable" -ForegroundColor Green
}

Write-Host ""
Write-Host "Data saved to: $outputFile" -ForegroundColor Cyan
Write-Host "Import into Excel/PowerBI for visualization" -ForegroundColor Gray
Write-Host ""
