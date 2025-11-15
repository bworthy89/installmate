using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Windows.Storage;
using Windows.System;

namespace InstallVibe.ViewModels;

public partial class DiagnosticsViewModel : ObservableObject
{
    [ObservableProperty]
    private string appVersion = string.Empty;

    [ObservableProperty]
    private string osVersion = string.Empty;

    [ObservableProperty]
    private string dotNetVersion = string.Empty;

    [ObservableProperty]
    private long memoryUsageMB;

    [ObservableProperty]
    private string databasePath = string.Empty;

    [ObservableProperty]
    private long databaseSizeMB;

    [ObservableProperty]
    private string logFilePath = string.Empty;

    [ObservableProperty]
    private long logFileSizeMB;

    [ObservableProperty]
    private bool isNetworkConnected;

    [ObservableProperty]
    private string lastUpdateCheck = string.Empty;

    [ObservableProperty]
    private int totalGuides;

    [ObservableProperty]
    private int totalSteps;

    [ObservableProperty]
    private bool isRefreshing;

    public ObservableCollection<LogEntry> RecentLogs { get; } = new();

    public DiagnosticsViewModel()
    {
        LoadDiagnosticInfo();
    }

    private async void LoadDiagnosticInfo()
    {
        await RefreshDiagnosticsAsync();
    }

    [RelayCommand]
    private async Task RefreshDiagnosticsAsync()
    {
        IsRefreshing = true;

        try
        {
            // Get app version
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            AppVersion = assembly.GetName().Version?.ToString() ?? "Unknown";

            // Get OS version
            var osVersionInfo = Environment.OSVersion;
            OsVersion = $"{osVersionInfo.VersionString} ({Environment.Is64BitOperatingSystem ? "x64" : "x86"})";

            // Get .NET version
            DotNetVersion = Environment.Version.ToString();

            // Get memory usage
            var process = System.Diagnostics.Process.GetCurrentProcess();
            MemoryUsageMB = process.WorkingSet64 / 1024 / 1024;

            // Get database info
            var localFolder = ApplicationData.Current.LocalFolder;
            DatabasePath = Path.Combine(localFolder.Path, "installvibe.db");

            if (File.Exists(DatabasePath))
            {
                var dbFileInfo = new FileInfo(DatabasePath);
                DatabaseSizeMB = dbFileInfo.Length / 1024 / 1024;
            }

            // Get log file info
            LogFilePath = Path.Combine(localFolder.Path, "logs", "app.log");

            if (File.Exists(LogFilePath))
            {
                var logFileInfo = new FileInfo(LogFilePath);
                LogFileSizeMB = logFileInfo.Length / 1024 / 1024;

                // Load recent log entries
                await LoadRecentLogsAsync();
            }

            // Network status
            IsNetworkConnected = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();

            // Last update check
            LastUpdateCheck = "Never"; // Would come from settings

            // Database stats
            // TotalGuides and TotalSteps would come from database query
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing diagnostics: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task OpenLogsFolderAsync()
    {
        try
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var logsPath = Path.Combine(localFolder.Path, "logs");

            if (Directory.Exists(logsPath))
            {
                await Launcher.LaunchFolderPathAsync(logsPath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening logs folder: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ExportDiagnosticsAsync()
    {
        try
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();

            // Get the current window's HWND
            var window = App.MainWindow;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Text File", new[] { ".txt" });
            savePicker.SuggestedFileName = $"installvibe-diagnostics-{DateTime.Now:yyyyMMdd-HHmmss}";

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                var diagnosticReport = GenerateDiagnosticReport();
                await FileIO.WriteTextAsync(file, diagnosticReport);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error exporting diagnostics: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        try
        {
            var localCacheFolder = ApplicationData.Current.LocalCacheFolder;
            var items = await localCacheFolder.GetItemsAsync();

            foreach (var item in items)
            {
                await item.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }

            await RefreshDiagnosticsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clearing cache: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ClearLogsAsync()
    {
        try
        {
            if (File.Exists(LogFilePath))
            {
                File.Delete(LogFilePath);
                RecentLogs.Clear();
                await RefreshDiagnosticsAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clearing logs: {ex.Message}");
        }
    }

    private async Task LoadRecentLogsAsync()
    {
        try
        {
            RecentLogs.Clear();

            if (!File.Exists(LogFilePath))
                return;

            var lines = await File.ReadAllLinesAsync(LogFilePath);
            var recentLines = lines.TakeLast(100).Reverse();

            foreach (var line in recentLines)
            {
                if (TryParseLogLine(line, out var logEntry))
                {
                    RecentLogs.Add(logEntry);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading recent logs: {ex.Message}");
        }
    }

    private bool TryParseLogLine(string line, out LogEntry logEntry)
    {
        logEntry = new LogEntry();

        try
        {
            // Expected format: [2024-01-15 14:30:22] [ERROR] Message
            if (line.Length < 25 || line[0] != '[')
                return false;

            var timestampEnd = line.IndexOf(']');
            if (timestampEnd == -1)
                return false;

            var timestamp = line.Substring(1, timestampEnd - 1);
            logEntry.Timestamp = DateTime.Parse(timestamp);

            var levelStart = line.IndexOf('[', timestampEnd);
            if (levelStart == -1)
                return false;

            var levelEnd = line.IndexOf(']', levelStart);
            if (levelEnd == -1)
                return false;

            var level = line.Substring(levelStart + 1, levelEnd - levelStart - 1).Trim();
            logEntry.Level = level;

            logEntry.Message = line.Substring(levelEnd + 2).Trim();

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateDiagnosticReport()
    {
        return $@"InstallVibe Diagnostic Report
Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

APPLICATION INFORMATION
=======================
Version: {AppVersion}
OS Version: {OsVersion}
.NET Version: {DotNetVersion}

PERFORMANCE
===========
Memory Usage: {MemoryUsageMB} MB
Database Size: {DatabaseSizeMB} MB
Log File Size: {LogFileSizeMB} MB

PATHS
=====
Database: {DatabasePath}
Logs: {LogFilePath}

STATUS
======
Network Connected: {IsNetworkConnected}
Last Update Check: {LastUpdateCheck}
Total Guides: {TotalGuides}
Total Steps: {TotalSteps}

RECENT ERRORS
=============
{string.Join(Environment.NewLine, RecentLogs.Where(l => l.Level == "ERROR").Take(10).Select(l => $"[{l.Timestamp:yyyy-MM-dd HH:mm:ss}] {l.Message}"))}

END OF REPORT
";
    }
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
