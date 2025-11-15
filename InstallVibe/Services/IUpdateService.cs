using System;
using System.Threading.Tasks;

namespace InstallVibe.Services
{
    /// <summary>
    /// Service for checking and managing application updates
    /// </summary>
    public interface IUpdateService
    {
        /// <summary>
        /// Event raised when update availability changes
        /// </summary>
        event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

        /// <summary>
        /// Event raised when update check completes
        /// </summary>
        event EventHandler<UpdateCheckCompletedEventArgs>? UpdateCheckCompleted;

        /// <summary>
        /// Gets whether an update is currently available
        /// </summary>
        bool IsUpdateAvailable { get; }

        /// <summary>
        /// Gets the latest available version, if any
        /// </summary>
        Version? LatestVersion { get; }

        /// <summary>
        /// Gets the currently installed version
        /// </summary>
        Version CurrentVersion { get; }

        /// <summary>
        /// Gets whether the available update is mandatory (forced update)
        /// </summary>
        bool IsMandatoryUpdate { get; }

        /// <summary>
        /// Gets whether an update check is currently in progress
        /// </summary>
        bool IsCheckingForUpdates { get; }

        /// <summary>
        /// Checks for available updates asynchronously
        /// </summary>
        /// <returns>True if an update is available, false otherwise</returns>
        Task<bool> CheckForUpdatesAsync();

        /// <summary>
        /// Gets update metadata including version, release notes, and size
        /// </summary>
        /// <returns>Update metadata if available, null otherwise</returns>
        Task<UpdateMetadata?> GetUpdateMetadataAsync();

        /// <summary>
        /// Initiates the update installation process
        /// This will typically restart the application to apply updates
        /// </summary>
        /// <returns>True if update initiation succeeded, false otherwise</returns>
        Task<bool> InstallUpdateAsync();

        /// <summary>
        /// Downloads update metadata for offline inspection without installing
        /// </summary>
        /// <returns>Downloaded metadata or null if unavailable</returns>
        Task<UpdateMetadata?> DownloadUpdateMetadataAsync();
    }

    /// <summary>
    /// Event args for update available notifications
    /// </summary>
    public class UpdateAvailableEventArgs : EventArgs
    {
        public Version NewVersion { get; set; } = new Version();
        public Version CurrentVersion { get; set; } = new Version();
        public bool IsMandatory { get; set; }
        public string? ReleaseNotes { get; set; }
        public long? DownloadSizeBytes { get; set; }
    }

    /// <summary>
    /// Event args for update check completion
    /// </summary>
    public class UpdateCheckCompletedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public bool UpdateAvailable { get; set; }
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// Metadata about an available update
    /// </summary>
    public class UpdateMetadata
    {
        public Version Version { get; set; } = new Version();
        public string? ReleaseNotes { get; set; }
        public DateTime? ReleasedDate { get; set; }
        public long? DownloadSizeBytes { get; set; }
        public bool IsMandatory { get; set; }
        public string? DownloadUrl { get; set; }
        public string? MinimumOSVersion { get; set; }
    }
}
