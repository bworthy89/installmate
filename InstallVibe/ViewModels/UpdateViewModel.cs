using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Services;
using Microsoft.Extensions.Logging;

namespace InstallVibe.ViewModels
{
    /// <summary>
    /// ViewModel for managing application updates
    ///
    /// This ViewModel demonstrates integration with UpdateService for:
    /// - Checking for updates
    /// - Displaying update notifications
    /// - Installing updates
    /// - Handling mandatory updates
    /// </summary>
    public partial class UpdateViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;
        private readonly INetworkService _networkService;
        private readonly ILogger<UpdateViewModel> _logger;

        [ObservableProperty]
        private bool _isUpdateAvailable;

        [ObservableProperty]
        private bool _isCheckingForUpdates;

        [ObservableProperty]
        private bool _isOfflineMode;

        [ObservableProperty]
        private string? _updateMessage;

        [ObservableProperty]
        private string? _currentVersion;

        [ObservableProperty]
        private string? _latestVersion;

        [ObservableProperty]
        private bool _isMandatoryUpdate;

        [ObservableProperty]
        private string? _releaseNotes;

        [ObservableProperty]
        private bool _showUpdateBanner;

        [ObservableProperty]
        private long? _downloadSizeBytes;

        public UpdateViewModel(
            IUpdateService updateService,
            INetworkService networkService,
            ILogger<UpdateViewModel> logger)
        {
            _updateService = updateService;
            _networkService = networkService;
            _logger = logger;

            // Initialize current version display
            CurrentVersion = _updateService.CurrentVersion.ToString();

            // Subscribe to update service events
            _updateService.UpdateAvailable += OnUpdateAvailable;
            _updateService.UpdateCheckCompleted += OnUpdateCheckCompleted;

            // Subscribe to network connectivity events
            _networkService.ConnectivityChanged += OnConnectivityChanged;

            // Initialize offline mode status
            IsOfflineMode = _networkService.IsOffline;

            _logger.LogInformation("UpdateViewModel initialized. Current version: {Version}", CurrentVersion);
        }

        /// <summary>
        /// Command to check for updates manually
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanCheckForUpdates))]
        private async Task CheckForUpdatesAsync()
        {
            try
            {
                _logger.LogInformation("Manual update check requested");

                IsCheckingForUpdates = true;
                UpdateMessage = "Checking for updates...";

                var updateAvailable = await _updateService.CheckForUpdatesAsync();

                if (!updateAvailable)
                {
                    UpdateMessage = "You're up to date! No updates available.";
                    ShowUpdateBanner = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for updates");
                UpdateMessage = "Failed to check for updates. Please try again later.";
            }
            finally
            {
                IsCheckingForUpdates = false;
            }
        }

        private bool CanCheckForUpdates()
        {
            return !IsCheckingForUpdates && !IsOfflineMode;
        }

        /// <summary>
        /// Command to install available update
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanInstallUpdate))]
        private async Task InstallUpdateAsync()
        {
            try
            {
                _logger.LogInformation("Installing update to version {Version}", LatestVersion);

                UpdateMessage = "Installing update...";

                var success = await _updateService.InstallUpdateAsync();

                if (success)
                {
                    UpdateMessage = "Update is being installed. The app will restart shortly.";

                    // Give user time to read the message
                    await Task.Delay(2000);

                    // Close app to allow update installation
                    // Windows will automatically apply the update and restart the app
                    Microsoft.UI.Xaml.Application.Current.Exit();
                }
                else
                {
                    UpdateMessage = "Failed to install update. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing update");
                UpdateMessage = "Failed to install update. Please try again.";
            }
        }

        private bool CanInstallUpdate()
        {
            return IsUpdateAvailable && !IsCheckingForUpdates && !IsOfflineMode;
        }

        /// <summary>
        /// Command to dismiss update notification (only for non-mandatory updates)
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanDismissUpdate))]
        private void DismissUpdate()
        {
            _logger.LogInformation("User dismissed update notification");
            ShowUpdateBanner = false;
            UpdateMessage = null;
        }

        private bool CanDismissUpdate()
        {
            return IsUpdateAvailable && !IsMandatoryUpdate;
        }

        /// <summary>
        /// Event handler for update availability
        /// </summary>
        private void OnUpdateAvailable(object? sender, UpdateAvailableEventArgs e)
        {
            _logger.LogInformation(
                "Update available: {NewVersion} (Current: {CurrentVersion}, Mandatory: {IsMandatory})",
                e.NewVersion, e.CurrentVersion, e.IsMandatory);

            IsUpdateAvailable = true;
            LatestVersion = e.NewVersion.ToString();
            IsMandatoryUpdate = e.IsMandatory;
            ReleaseNotes = e.ReleaseNotes;
            DownloadSizeBytes = e.DownloadSizeBytes;
            ShowUpdateBanner = true;

            // Format update message
            var sizeText = e.DownloadSizeBytes.HasValue
                ? $" ({FormatBytes(e.DownloadSizeBytes.Value)})"
                : "";

            UpdateMessage = e.IsMandatory
                ? $"⚠️ Critical update required: Version {LatestVersion}{sizeText}. Please install now."
                : $"🎉 Update available: Version {LatestVersion}{sizeText}";

            // Refresh command states
            CheckForUpdatesCommand.NotifyCanExecuteChanged();
            InstallUpdateCommand.NotifyCanExecuteChanged();
            DismissUpdateCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Event handler for update check completion
        /// </summary>
        private void OnUpdateCheckCompleted(object? sender, UpdateCheckCompletedEventArgs e)
        {
            IsCheckingForUpdates = false;

            if (!e.Success)
            {
                _logger.LogWarning("Update check failed: {Error}", e.ErrorMessage);

                if (!string.IsNullOrEmpty(e.ErrorMessage))
                {
                    UpdateMessage = $"Update check failed: {e.ErrorMessage}";
                }
            }
            else if (!e.UpdateAvailable)
            {
                _logger.LogInformation("No updates available");
            }

            // Refresh command states
            CheckForUpdatesCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Event handler for network connectivity changes
        /// </summary>
        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            _logger.LogInformation("Network connectivity changed: {Status}", e.IsConnected ? "Online" : "Offline");

            IsOfflineMode = !e.IsConnected;

            // Update UI message
            if (IsOfflineMode)
            {
                UpdateMessage = "⚠️ Offline mode — Updates unavailable";
                ShowUpdateBanner = true;
            }
            else if (ShowUpdateBanner && UpdateMessage?.Contains("Offline") == true)
            {
                // Clear offline message when back online
                UpdateMessage = null;
                ShowUpdateBanner = false;
            }

            // Refresh command states
            CheckForUpdatesCommand.NotifyCanExecuteChanged();
            InstallUpdateCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Formats bytes to human-readable string
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Cleanup when ViewModel is disposed
        /// </summary>
        public void Cleanup()
        {
            _updateService.UpdateAvailable -= OnUpdateAvailable;
            _updateService.UpdateCheckCompleted -= OnUpdateCheckCompleted;
            _networkService.ConnectivityChanged -= OnConnectivityChanged;
        }
    }
}
