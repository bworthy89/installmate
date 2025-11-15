using System;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Windows.Networking.Connectivity;

namespace InstallVibe.Services
{
    /// <summary>
    /// Implementation of network connectivity service
    ///
    /// Provides offline mode detection critical for factory floor deployments
    /// where network connectivity may be intermittent or unavailable
    /// </summary>
    public class NetworkService : INetworkService
    {
        private readonly ILogger<NetworkService> _logger;
        private readonly HttpClient _httpClient;
        private bool _isMonitoring;

        public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;

        public bool IsConnected { get; private set; }

        public NetworkService(ILogger<NetworkService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;

            // Initialize connectivity status
            UpdateConnectivityStatus();
        }

        /// <summary>
        /// Checks if the device has network connectivity
        /// Uses fast local checks first, then optional internet connectivity verification
        /// </summary>
        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                // Quick check: Is any network interface up?
                if (!IsAnyNetworkInterfaceUp())
                {
                    _logger.LogDebug("No network interfaces are up");
                    UpdateConnectivity(false);
                    return false;
                }

                // Check Windows connection profile
                var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
                if (connectionProfile == null)
                {
                    _logger.LogDebug("No internet connection profile available");
                    UpdateConnectivity(false);
                    return false;
                }

                var connectivityLevel = connectionProfile.GetNetworkConnectivityLevel();
                var hasInternetAccess = connectivityLevel == NetworkConnectivityLevel.InternetAccess;

                // Optional: Verify actual internet connectivity with a quick HTTP check
                // For factory floors with intranet-only access, you may want to disable this
                // or check a local server instead
                if (hasInternetAccess)
                {
                    // Quick verification ping to a reliable endpoint
                    // Using HTTP HEAD for minimal data transfer
                    hasInternetAccess = await VerifyInternetConnectivityAsync();
                }

                UpdateConnectivity(hasInternetAccess);
                return hasInternetAccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking network connectivity");
                UpdateConnectivity(false);
                return false;
            }
        }

        /// <summary>
        /// Checks if a specific host is reachable
        /// Useful for checking internal servers or update endpoints
        /// </summary>
        public async Task<bool> IsHostReachableAsync(string host, int timeoutMs = 5000)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host, timeoutMs);
                var isReachable = reply.Status == IPStatus.Success;

                _logger.LogDebug("Ping to {Host}: {Status} ({RoundTrip}ms)",
                    host, reply.Status, reply.RoundtripTime);

                return isReachable;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to ping host {Host}", host);
                return false;
            }
        }

        /// <summary>
        /// Gets the current network connection type
        /// </summary>
        public NetworkConnectionType GetConnectionType()
        {
            try
            {
                var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
                if (connectionProfile == null)
                {
                    return NetworkConnectionType.None;
                }

                // Check connection cost to infer connection type
                var cost = connectionProfile.GetConnectionCost();
                if (cost.Roaming || cost.NetworkCostType == NetworkCostType.Variable)
                {
                    return NetworkConnectionType.Cellular;
                }

                // Check network adapter type
                var networkAdapter = connectionProfile.NetworkAdapter;
                if (networkAdapter != null)
                {
                    var ianaType = networkAdapter.IanaInterfaceType;

                    return ianaType switch
                    {
                        6 => NetworkConnectionType.Ethernet,    // Ethernet
                        71 => NetworkConnectionType.WiFi,        // IEEE 802.11 wireless
                        243 => NetworkConnectionType.Cellular,   // Mobile broadband
                        244 => NetworkConnectionType.Cellular,   // Mobile broadband
                        _ => NetworkConnectionType.Unknown
                    };
                }

                return NetworkConnectionType.Unknown;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error determining connection type");
                return NetworkConnectionType.Unknown;
            }
        }

        /// <summary>
        /// Starts monitoring network connectivity changes
        /// </summary>
        public void StartMonitoring()
        {
            if (_isMonitoring)
            {
                _logger.LogWarning("Network monitoring is already active");
                return;
            }

            _logger.LogInformation("Starting network connectivity monitoring");

            NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;
            _isMonitoring = true;
        }

        /// <summary>
        /// Stops monitoring network connectivity changes
        /// </summary>
        public void StopMonitoring()
        {
            if (!_isMonitoring)
            {
                return;
            }

            _logger.LogInformation("Stopping network connectivity monitoring");

            NetworkInformation.NetworkStatusChanged -= OnNetworkStatusChanged;
            _isMonitoring = false;
        }

        /// <summary>
        /// Event handler for Windows network status changes
        /// </summary>
        private async void OnNetworkStatusChanged(object? sender)
        {
            _logger.LogDebug("Network status changed event received");

            // Re-check connectivity
            await IsConnectedAsync();
        }

        /// <summary>
        /// Quick check if any network interface is up
        /// </summary>
        private bool IsAnyNetworkInterfaceUp()
        {
            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                return interfaces.Any(ni =>
                    ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking network interfaces");
                return false;
            }
        }

        /// <summary>
        /// Verifies actual internet connectivity with a quick HTTP request
        /// For intranet-only environments, modify this to check your internal server
        /// </summary>
        private async Task<bool> VerifyInternetConnectivityAsync()
        {
            try
            {
                // Use a reliable, fast endpoint
                // For factory floor with intranet-only, replace with your update server
                var checkUrl = "https://www.msftconnecttest.com/connecttest.txt";

                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                var response = await _httpClient.GetAsync(checkUrl, cts.Token);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                // Don't log - this is an optional verification
                return false;
            }
        }

        /// <summary>
        /// Updates connectivity status and raises events if changed
        /// </summary>
        private void UpdateConnectivityStatus()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            var hasConnection = connectionProfile != null &&
                               connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;

            UpdateConnectivity(hasConnection);
        }

        /// <summary>
        /// Updates connectivity and raises event if status changed
        /// </summary>
        private void UpdateConnectivity(bool isConnected)
        {
            var previousStatus = IsConnected;
            IsConnected = isConnected;

            if (previousStatus != isConnected)
            {
                _logger.LogInformation("Network connectivity changed: {Status}",
                    isConnected ? "Connected" : "Offline");

                ConnectivityChanged?.Invoke(this, new ConnectivityChangedEventArgs
                {
                    IsConnected = isConnected,
                    ConnectionType = GetConnectionType()
                });
            }
        }
    }
}
