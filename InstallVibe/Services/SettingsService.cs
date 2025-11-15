using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace InstallVibe.Services;

public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private Settings _settings;

    public ElementTheme Theme
    {
        get => _settings.Theme;
        set
        {
            _settings.Theme = value;
            ApplyTheme(value);
        }
    }

    public string FontSize
    {
        get => _settings.FontSize;
        set => _settings.FontSize = value;
    }

    public bool TelemetryEnabled
    {
        get => _settings.TelemetryEnabled;
        set => _settings.TelemetryEnabled = value;
    }

    public SettingsService()
    {
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataFolder, "InstallVibe");
        Directory.CreateDirectory(appFolder);
        _settingsPath = Path.Combine(appFolder, "Settings.json");

        _settings = new Settings
        {
            Theme = ElementTheme.Default,
            FontSize = "Normal",
            TelemetryEnabled = false
        };
    }

    public async Task LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = await File.ReadAllTextAsync(_settingsPath);
                _settings = JsonSerializer.Deserialize<Settings>(json) ?? _settings;
                ApplyTheme(_settings.Theme);
            }
        }
        catch
        {
            // If loading fails, use defaults
            _settings = new Settings
            {
                Theme = ElementTheme.Default,
                FontSize = "Normal",
                TelemetryEnabled = false
            };
        }
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_settingsPath, json);
        }
        catch
        {
            // Silently fail if unable to save
        }
    }

    public async Task ClearAllDataAsync()
    {
        try
        {
            // Clear database
            using var scope = App.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Data.InstallVibeDbContext>();
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            // Re-seed admin user
            var authService = App.Services.GetRequiredService<IAuthService>();
            await authService.SeedAdmin();
        }
        catch
        {
            throw new InvalidOperationException("Failed to clear application data");
        }
    }

    private void ApplyTheme(ElementTheme theme)
    {
        if (App.MainWindow?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = theme;
        }
    }

    private class Settings
    {
        public ElementTheme Theme { get; set; }
        public string FontSize { get; set; } = "Normal";
        public bool TelemetryEnabled { get; set; }
    }
}
