using System;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InstallVibe.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private int _selectedThemeIndex;

    [ObservableProperty]
    private int _selectedFontSizeIndex;

    [ObservableProperty]
    private bool _telemetryEnabled;

    [ObservableProperty]
    private string _appVersion = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SettingsViewModel(ISettingsService settingsService, INavigationService navigationService)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;

        // Get app version from assembly
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        AppVersion = $"Version {version?.Major}.{version?.Minor}.{version?.Build}";

        LoadSettings();
    }

    private void LoadSettings()
    {
        // Map theme to index
        SelectedThemeIndex = _settingsService.Theme switch
        {
            ElementTheme.Light => 0,
            ElementTheme.Dark => 1,
            ElementTheme.Default => 2,
            _ => 2
        };

        // Map font size to index
        SelectedFontSizeIndex = _settingsService.FontSize switch
        {
            "Small" => 0,
            "Normal" => 1,
            "Large" => 2,
            _ => 1
        };

        TelemetryEnabled = _settingsService.TelemetryEnabled;
    }

    partial void OnSelectedThemeIndexChanged(int value)
    {
        var theme = value switch
        {
            0 => ElementTheme.Light,
            1 => ElementTheme.Dark,
            2 => ElementTheme.Default,
            _ => ElementTheme.Default
        };

        _settingsService.Theme = theme;
        _ = _settingsService.SaveSettingsAsync();
    }

    partial void OnSelectedFontSizeIndexChanged(int value)
    {
        var fontSize = value switch
        {
            0 => "Small",
            1 => "Normal",
            2 => "Large",
            _ => "Normal"
        };

        _settingsService.FontSize = fontSize;
        _ = _settingsService.SaveSettingsAsync();
        StatusMessage = "Font size will take effect after app restart";
    }

    partial void OnTelemetryEnabledChanged(bool value)
    {
        _settingsService.TelemetryEnabled = value;
        _ = _settingsService.SaveSettingsAsync();
    }

    [RelayCommand]
    private async Task ClearAllData()
    {
        try
        {
            await _settingsService.ClearAllDataAsync();
            StatusMessage = "All data cleared successfully. Please restart the application.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error clearing data: {ex.Message}";
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }
}
