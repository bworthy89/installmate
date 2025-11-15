using System;
using System.Threading.Tasks;

namespace InstallVibe.Services
{
    /// <summary>
    /// Service for detecting network connectivity and online/offline mode
    /// </summary>
    public interface INetworkService
    {
        /// <summary>
        /// Event raised when connectivity status changes
        /// </summary>
        event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;

        /// <summary>
        /// Gets whether the device currently has network connectivity
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets whether the device is currently in offline mode
        /// </summary>
        bool IsOffline => !IsConnected;

        /// <summary>
        /// Checks if the device has network connectivity
        /// </summary>
        /// <returns>True if connected, false otherwise</returns>
        Task<bool> IsConnectedAsync();

        /// <summary>
        /// Checks if a specific host is reachable
        /// </summary>
        /// <param name="host">Host to check (e.g., "google.com" or "internal-server.local")</param>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>True if host is reachable, false otherwise</returns>
        Task<bool> IsHostReachableAsync(string host, int timeoutMs = 5000);

        /// <summary>
        /// Gets current network connection type (WiFi, Ethernet, Cellular, etc.)
        /// </summary>
        /// <returns>Connection type</returns>
        NetworkConnectionType GetConnectionType();

        /// <summary>
        /// Starts monitoring network connectivity changes
        /// </summary>
        void StartMonitoring();

        /// <summary>
        /// Stops monitoring network connectivity changes
        /// </summary>
        void StopMonitoring();
    }

    /// <summary>
    /// Event args for connectivity changes
    /// </summary>
    public class ConnectivityChangedEventArgs : EventArgs
    {
        public bool IsConnected { get; set; }
        public NetworkConnectionType ConnectionType { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Network connection types
    /// </summary>
    public enum NetworkConnectionType
    {
        None,
        Unknown,
        Ethernet,
        WiFi,
        Cellular,
        Bluetooth
    }
}
