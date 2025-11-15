using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace InstallVibe.Services
{
    /// <summary>
    /// Implementation of update service for MSIX-packaged applications
    ///
    /// This service checks for updates by fetching the appinstaller XML file
    /// and comparing versions. For MSIX apps, Windows handles the actual update
    /// download and installation automatically.
    /// </summary>
    public class UpdateService : IUpdateService
    {
        private readonly ILogger<UpdateService> _logger;
        private readonly INetworkService _networkService;
        private readonly HttpClient _httpClient;
        private readonly string _appInstallerUrl;

        private bool _isUpdateAvailable;
        private Version? _latestVersion;
        private bool _isMandatoryUpdate;
        private bool _isCheckingForUpdates;

        /// <summary>
        /// Default App Installer URL
        /// Override this in production with your actual hosting URL
        /// </summary>
        private const string DefaultAppInstallerUrl = "https://updates.yourcompany.com/installvibe/InstallVibe.appinstaller";

        public event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;
        public event EventHandler<UpdateCheckCompletedEventArgs>? UpdateCheckCompleted;

        public bool IsUpdateAvailable => _isUpdateAvailable;
        public Version? LatestVersion => _latestVersion;
        public Version CurrentVersion { get; private set; }
        public bool IsMandatoryUpdate => _isMandatoryUpdate;
        public bool IsCheckingForUpdates => _isCheckingForUpdates;

        public UpdateService(
            ILogger<UpdateService> logger,
            INetworkService networkService,
            HttpClient httpClient)
        {
            _logger = logger;
            _networkService = networkService;
            _httpClient = httpClient;

            // Get current version from assembly
            CurrentVersion = GetCurrentVersion();

            // Get App Installer URL from configuration or use default
            // In production, inject this via configuration service
            _appInstallerUrl = GetAppInstallerUrlFromConfig() ?? DefaultAppInstallerUrl;

            _logger.LogInformation("UpdateService initialized. Current version: {Version}, Update URL: {Url}",
                CurrentVersion, _appInstallerUrl);
        }

        /// <summary>
        /// Checks for available updates by fetching and parsing the appinstaller file
        /// </summary>
        public async Task<bool> CheckForUpdatesAsync()
        {
            if (_isCheckingForUpdates)
            {
                _logger.LogWarning("Update check already in progress");
                return false;
            }

            _isCheckingForUpdates = true;

            try
            {
                _logger.LogInformation("Checking for updates from {Url}", _appInstallerUrl);

                // Check network connectivity first
                if (!await _networkService.IsConnectedAsync())
                {
                    _logger.LogWarning("No network connectivity - skipping update check");
                    RaiseUpdateCheckCompleted(false, false, "No network connectivity");
                    return false;
                }

                // Fetch appinstaller XML
                var appInstallerXml = await FetchAppInstallerXmlAsync();
                if (appInstallerXml == null)
                {
                    _logger.LogWarning("Failed to fetch appinstaller file");
                    RaiseUpdateCheckCompleted(false, false, "Failed to fetch update information");
                    return false;
                }

                // Parse version information
                var metadata = ParseAppInstallerXml(appInstallerXml);
                if (metadata == null)
                {
                    _logger.LogWarning("Failed to parse appinstaller file");
                    RaiseUpdateCheckCompleted(false, false, "Invalid update information");
                    return false;
                }

                // Compare versions
                _latestVersion = metadata.Version;
                _isMandatoryUpdate = metadata.IsMandatory;
                _isUpdateAvailable = _latestVersion > CurrentVersion;

                _logger.LogInformation(
                    "Update check completed. Current: {Current}, Latest: {Latest}, Available: {Available}, Mandatory: {Mandatory}",
                    CurrentVersion, _latestVersion, _isUpdateAvailable, _isMandatoryUpdate);

                // Raise events
                if (_isUpdateAvailable)
                {
                    RaiseUpdateAvailable(metadata);
                }

                RaiseUpdateCheckCompleted(true, _isUpdateAvailable, null);

                return _isUpdateAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for updates");
                RaiseUpdateCheckCompleted(false, false, ex.Message, ex);
                return false;
            }
            finally
            {
                _isCheckingForUpdates = false;
            }
        }

        /// <summary>
        /// Gets detailed update metadata
        /// </summary>
        public async Task<UpdateMetadata?> GetUpdateMetadataAsync()
        {
            try
            {
                var xml = await FetchAppInstallerXmlAsync();
                return xml != null ? ParseAppInstallerXml(xml) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching update metadata");
                return null;
            }
        }

        /// <summary>
        /// Downloads update metadata without installing
        /// </summary>
        public async Task<UpdateMetadata?> DownloadUpdateMetadataAsync()
        {
            return await GetUpdateMetadataAsync();
        }

        /// <summary>
        /// Initiates the update installation process
        ///
        /// For MSIX apps, we can't directly trigger installation, but we can:
        /// 1. Launch the appinstaller file to prompt user
        /// 2. Or close the app to let Windows auto-update on next launch
        /// </summary>
        public async Task<bool> InstallUpdateAsync()
        {
            if (!_isUpdateAvailable)
            {
                _logger.LogWarning("No update available to install");
                return false;
            }

            try
            {
                _logger.LogInformation("Initiating update installation");

                // For MSIX apps, the cleanest approach is to close the app
                // and let Windows handle the update on next launch
                // Alternatively, launch the appinstaller URL to prompt user

                // Option 1: Launch appinstaller (requires user interaction)
                var processInfo = new ProcessStartInfo
                {
                    FileName = _appInstallerUrl,
                    UseShellExecute = true
                };
                Process.Start(processInfo);

                _logger.LogInformation("Launched app installer: {Url}", _appInstallerUrl);

                // Option 2: Close app for automatic update
                // Application.Current.Exit();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating update installation");
                return false;
            }
        }

        /// <summary>
        /// Fetches the appinstaller XML file from the configured URL
        /// </summary>
        private async Task<string?> FetchAppInstallerXmlAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_appInstallerUrl);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching appinstaller file from {Url}", _appInstallerUrl);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appinstaller file");
                return null;
            }
        }

        /// <summary>
        /// Parses the appinstaller XML to extract update metadata
        /// </summary>
        private UpdateMetadata? ParseAppInstallerXml(string xml)
        {
            try
            {
                var doc = XDocument.Parse(xml);
                var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

                // Get MainBundle element
                var mainBundle = doc.Root?.Element(ns + "MainBundle");
                if (mainBundle == null)
                {
                    _logger.LogWarning("MainBundle element not found in appinstaller file");
                    return null;
                }

                // Extract version
                var versionAttr = mainBundle.Attribute("Version")?.Value;
                if (!Version.TryParse(versionAttr, out var version))
                {
                    _logger.LogWarning("Failed to parse version from appinstaller: {Version}", versionAttr);
                    return null;
                }

                // Extract update settings
                var updateSettings = doc.Root?.Element(ns + "UpdateSettings");
                var forceUpdate = updateSettings?.Element(ns + "ForceUpdateFromAnyVersion")?.Value;
                var isMandatory = bool.TryParse(forceUpdate, out var mandatory) && mandatory;

                // Extract download URL
                var downloadUrl = mainBundle.Attribute("Uri")?.Value;

                var metadata = new UpdateMetadata
                {
                    Version = version,
                    IsMandatory = isMandatory,
                    DownloadUrl = downloadUrl,
                    ReleasedDate = DateTime.UtcNow // appinstaller doesn't include release date
                };

                _logger.LogDebug("Parsed update metadata: Version={Version}, Mandatory={Mandatory}, Url={Url}",
                    metadata.Version, metadata.IsMandatory, metadata.DownloadUrl);

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing appinstaller XML");
                return null;
            }
        }

        /// <summary>
        /// Gets the current application version
        /// </summary>
        private Version GetCurrentVersion()
        {
            try
            {
                // Try to get package version (MSIX)
                var package = Windows.ApplicationModel.Package.Current;
                var packageVersion = package.Id.Version;
                return new Version(
                    packageVersion.Major,
                    packageVersion.Minor,
                    packageVersion.Build,
                    packageVersion.Revision);
            }
            catch
            {
                // Fallback to assembly version (non-packaged)
                var assembly = Assembly.GetExecutingAssembly();
                return assembly.GetName().Version ?? new Version(1, 0, 0, 0);
            }
        }

        /// <summary>
        /// Gets the App Installer URL from configuration
        /// Override this in production to read from app settings or registry
        /// </summary>
        private string? GetAppInstallerUrlFromConfig()
        {
            // TODO: Read from configuration service or app settings
            // For now, return null to use default
            // In production, this could read from:
            // - appsettings.json
            // - Registry (for IT-managed deployments)
            // - Environment variable
            // - Remote configuration service

            return null;
        }

        private void RaiseUpdateAvailable(UpdateMetadata metadata)
        {
            UpdateAvailable?.Invoke(this, new UpdateAvailableEventArgs
            {
                NewVersion = metadata.Version,
                CurrentVersion = CurrentVersion,
                IsMandatory = metadata.IsMandatory,
                ReleaseNotes = metadata.ReleaseNotes,
                DownloadSizeBytes = metadata.DownloadSizeBytes
            });
        }

        private void RaiseUpdateCheckCompleted(bool success, bool updateAvailable, string? errorMessage, Exception? exception = null)
        {
            UpdateCheckCompleted?.Invoke(this, new UpdateCheckCompletedEventArgs
            {
                Success = success,
                UpdateAvailable = updateAvailable,
                ErrorMessage = errorMessage,
                Exception = exception
            });
        }
    }
}
