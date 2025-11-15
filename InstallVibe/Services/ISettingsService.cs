using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace InstallVibe.Services;

public interface ISettingsService
{
    ElementTheme Theme { get; set; }
    string FontSize { get; set; }
    bool TelemetryEnabled { get; set; }

    Task LoadSettingsAsync();
    Task SaveSettingsAsync();
    Task ClearAllDataAsync();
}
