<#
.SYNOPSIS
    Load test: Creates 500 guides in the database

.DESCRIPTION
    This script populates the InstallVibe database with 500 test guides
    to verify performance with large datasets.

.PARAMETER DatabasePath
    Path to the InstallVibe SQLite database

.EXAMPLE
    .\LoadManyGuides.ps1 -DatabasePath "C:\Users\tech\AppData\Local\InstallVibe\installvibe.db"
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$DatabasePath = "$env:LOCALAPPDATA\InstallVibe\installvibe.db"
)

$ErrorActionPreference = "Stop"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "InstallVibe Load Test: 500 Guides" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check if SQLite module is available
if (-not (Get-Command "Invoke-SqliteQuery" -ErrorAction SilentlyContinue)) {
    Write-Host "Installing PSSQLite module..." -ForegroundColor Yellow
    Install-Module -Name PSSQLite -Force -Scope CurrentUser
    Import-Module PSSQLite
}

# Verify database exists
if (-not (Test-Path $DatabasePath)) {
    Write-Error "Database not found at: $DatabasePath"
    exit 1
}

Write-Host "Database: $DatabasePath" -ForegroundColor Gray
Write-Host ""

$categories = @("Mechanical", "Electrical", "Pneumatic", "Hydraulic", "Software", "Safety")
$difficulties = @("Beginner", "Intermediate", "Advanced", "Expert")
$tools = @("Wrench", "Screwdriver", "Multimeter", "Torque Wrench", "Allen Key", "Pliers")

$startTime = Get-Date
Write-Host "Creating 500 guides..." -ForegroundColor Green

for ($i = 1; $i -le 500; $i++) {
    $category = $categories | Get-Random
    $difficulty = $difficulties | Get-Random
    $estimatedMinutes = Get-Random -Minimum 15 -Maximum 180
    $stepCount = Get-Random -Minimum 5 -Maximum 50

    # Insert guide
    $guideQuery = @"
INSERT INTO Guides (Title, Description, Category, EstimatedMinutes, DifficultyLevel, CreatedDate)
VALUES (
    'Test Guide $i - $category Installation',
    'This is a test guide for load testing. Guide number $i in category $category.',
    '$category',
    $estimatedMinutes,
    '$difficulty',
    datetime('now')
);
"@

    Invoke-SqliteQuery -DataSource $DatabasePath -Query $guideQuery

    # Get the inserted guide ID
    $guideId = Invoke-SqliteQuery -DataSource $DatabasePath -Query "SELECT last_insert_rowid() as Id"
    $guideIdValue = $guideId.Id

    # Insert steps for this guide
    for ($step = 1; $step -le $stepCount; $step++) {
        $stepQuery = @"
INSERT INTO Steps (GuideId, StepNumber, Title, Description, MediaType, EstimatedMinutes)
VALUES (
    $guideIdValue,
    $step,
    'Step $step: Test Action',
    'This is test step $step for guide $i. Perform the required action and verify completion.',
    'Image',
    $(Get-Random -Minimum 2 -Maximum 10)
);
"@
        Invoke-SqliteQuery -DataSource $DatabasePath -Query $stepQuery
    }

    # Progress indicator
    if ($i % 50 -eq 0) {
        $elapsed = ((Get-Date) - $startTime).TotalSeconds
        $rate = $i / $elapsed
        $remaining = (500 - $i) / $rate
        Write-Host "  Created $i guides ($([int]$rate) guides/sec, ETA: $([int]$remaining)s)" -ForegroundColor Gray
    }
}

$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Load Test Complete!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Guides created: 500" -ForegroundColor Yellow
Write-Host "Duration: $([int]$duration) seconds" -ForegroundColor Yellow
Write-Host "Rate: $([int](500/$duration)) guides/second" -ForegroundColor Yellow
Write-Host ""

# Verify count
$count = Invoke-SqliteQuery -DataSource $DatabasePath -Query "SELECT COUNT(*) as Total FROM Guides"
Write-Host "Total guides in database: $($count.Total)" -ForegroundColor Cyan
Write-Host ""
