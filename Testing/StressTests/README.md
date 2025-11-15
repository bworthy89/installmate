# InstallVibe Stress Tests

PowerShell scripts for stress testing, load testing, and reliability validation.

## Scripts Overview

| Script | Purpose | Duration | Requirements |
|--------|---------|----------|--------------|
| **LoadManyGuides.ps1** | Create 500 test guides | ~2 minutes | PSSQLite module |
| **RepeatedStepLoop.ps1** | Navigate guide 100x | ~30 minutes | UIAutomation module |
| **Stability48hr.ps1** | 48-hour stability test | 48 hours | Running app |
| **MemoryLeakDetection.ps1** | Memory leak detection | 1-4 hours | Running app |

## Prerequisites

### Install Required Modules

```powershell
# Install PSSQLite for database operations
Install-Module -Name PSSQLite -Force -Scope CurrentUser

# Install UIAutomation for UI testing
Install-Module -Name UIAutomation -Force -Scope CurrentUser
```

### Prepare Test Environment

1. Install InstallVibe from MSIX package
2. Launch the application and login
3. Ensure you have sufficient disk space for logs

## Running Tests

### 1. Load Test: Create 500 Guides

Tests database performance with large datasets.

```powershell
# Use default database path
.\LoadManyGuides.ps1

# Specify custom database path
.\LoadManyGuides.ps1 -DatabasePath "C:\Custom\Path\installvibe.db"
```

**Expected Results:**
- ✅ All 500 guides created successfully
- ✅ Database query time remains under 100ms
- ✅ Application loads guide list in under 2 seconds

### 2. Stress Test: Repeated Navigation

Tests for memory leaks and UI stability through repeated navigation.

```powershell
# Navigate through guide 1, 100 times
.\RepeatedStepLoop.ps1 -GuideId 1 -Iterations 100

# Custom iteration count
.\RepeatedStepLoop.ps1 -GuideId 5 -Iterations 200
```

**Expected Results:**
- ✅ No crashes after 100 iterations
- ✅ Memory growth under 100 MB
- ✅ UI remains responsive

### 3. 48-Hour Stability Test

Monitors application health continuously for 48 hours.

```powershell
# Default settings (check every 30 minutes)
.\Stability48hr.ps1

# Custom log path and check interval
.\Stability48hr.ps1 -LogPath "C:\Logs\stability.txt" -CheckIntervalMinutes 15
```

**Expected Results:**
- ✅ Zero crashes over 48 hours
- ✅ Zero hangs or freezes
- ✅ Memory remains under 500 MB
- ✅ Handle count stable (no handle leaks)

**Outputs:**
- `stability-test-log.txt` - Detailed event log
- `stability-test-log.csv` - Performance metrics (Time, Memory, CPU, Threads, Handles)

### 4. Memory Leak Detection

Detailed memory profiling to detect leaks.

```powershell
# Run for 60 minutes (default)
.\MemoryLeakDetection.ps1

# Run for 2 hours with 5-second sampling
.\MemoryLeakDetection.ps1 -DurationMinutes 120 -SampleIntervalSeconds 5
```

**Expected Results:**
- ✅ Memory growth rate under 0.5 MB/minute
- ✅ No continuous upward trend
- ✅ Memory stabilizes after initial warmup

**Outputs:**
- `memory-leak-test-YYYYMMDD-HHMMSS.csv` - Time-series memory data

## Analyzing Results

### Memory Leak Detection

Import the CSV into Excel or Python for visualization:

```python
import pandas as pd
import matplotlib.pyplot as plt

# Load data
df = pd.read_csv('memory-leak-test-20240115-143022.csv')

# Plot memory over time
plt.figure(figsize=(12, 6))
plt.plot(df['Sample'], df['Memory_MB'], label='Working Set')
plt.plot(df['Sample'], df['Private_MB'], label='Private Memory')
plt.xlabel('Sample Number')
plt.ylabel('Memory (MB)')
plt.title('Memory Usage Over Time')
plt.legend()
plt.grid(True)
plt.savefig('memory-trend.png')
plt.show()
```

### 48-Hour Stability Analysis

Check the CSV for trends:

```powershell
# Import data
$data = Import-Csv -Path "stability-test-log.csv"

# Calculate statistics
$avgMemory = ($data | Measure-Object -Property Memory_MB -Average).Average
$maxMemory = ($data | Measure-Object -Property Memory_MB -Maximum).Maximum
$avgThreads = ($data | Measure-Object -Property Threads -Average).Average

Write-Host "Average Memory: $([int]$avgMemory) MB"
Write-Host "Peak Memory: $([int]$maxMemory) MB"
Write-Host "Average Threads: $([int]$avgThreads)"
```

## Performance Targets

Based on QA test plan requirements:

| Metric | Target | Test |
|--------|--------|------|
| **Cold start** | < 3 seconds | Manual timing |
| **Guide load (200 steps)** | < 2 seconds | LoadManyGuides.ps1 |
| **Step navigation** | < 500ms | RepeatedStepLoop.ps1 |
| **Memory (idle)** | < 150 MB | MemoryLeakDetection.ps1 |
| **Memory (large guide)** | < 350 MB | MemoryLeakDetection.ps1 |
| **Memory growth** | < 0.5 MB/min | MemoryLeakDetection.ps1 |
| **48hr stability** | 0 crashes | Stability48hr.ps1 |

## Troubleshooting

### "Module not found" errors

```powershell
# Install missing modules
Install-Module -Name PSSQLite -Force -Scope CurrentUser
Install-Module -Name UIAutomation -Force -Scope CurrentUser
```

### "Database not found" errors

Check the database path:

```powershell
$dbPath = "$env:LOCALAPPDATA\InstallVibe\installvibe.db"
Test-Path $dbPath  # Should return True
```

### "Window not found" errors

Ensure InstallVibe is running and visible:

```powershell
Get-Process -Name "InstallVibe"
```

### High memory usage

If memory exceeds 500 MB consistently:
1. Check for open guides with large images
2. Review media caching strategy
3. Run MemoryLeakDetection.ps1 for detailed analysis

## CI/CD Integration

### GitHub Actions Example

```yaml
- name: Run Load Test
  run: |
    .\Testing\StressTests\LoadManyGuides.ps1
    if ($LASTEXITCODE -ne 0) { exit 1 }

- name: Run Memory Leak Test
  run: |
    Start-Process InstallVibe.exe
    Start-Sleep -Seconds 10
    .\Testing\StressTests\MemoryLeakDetection.ps1 -DurationMinutes 30
```

### Azure DevOps Example

```yaml
- task: PowerShell@2
  displayName: 'Load Test: 500 Guides'
  inputs:
    filePath: 'Testing/StressTests/LoadManyGuides.ps1'
    failOnStderr: true

- task: PowerShell@2
  displayName: 'Memory Leak Detection'
  inputs:
    filePath: 'Testing/StressTests/MemoryLeakDetection.ps1'
    arguments: '-DurationMinutes 60'
```

## Best Practices

1. **Run on dedicated test machine** - Don't run on development machine
2. **Close other applications** - Reduce interference from other processes
3. **Use release builds** - Test performance-optimized builds, not debug
4. **Baseline first** - Run tests on known-good version for comparison
5. **Automate nightly** - Schedule stability tests to run overnight
6. **Monitor trends** - Track metrics over time, not just single runs

## Cleanup

After testing:

```powershell
# Remove test guides from database
$dbPath = "$env:LOCALAPPDATA\InstallVibe\installvibe.db"
Invoke-SqliteQuery -DataSource $dbPath -Query "DELETE FROM Guides WHERE Title LIKE 'Test Guide%'"

# Delete log files
Remove-Item .\*.csv
Remove-Item .\*.txt
```
