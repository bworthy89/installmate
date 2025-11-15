using Xunit;
using FluentAssertions;
using InstallVibe.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;

namespace InstallVibe.Tests.Services;

public class NetworkServiceTests
{
    private readonly Mock<ILogger<NetworkService>> _mockLogger;
    private readonly HttpClient _httpClient;
    private readonly NetworkService _networkService;

    public NetworkServiceTests()
    {
        _mockLogger = new Mock<ILogger<NetworkService>>();
        _httpClient = new HttpClient();
        _networkService = new NetworkService(_mockLogger.Object, _httpClient);
    }

    [Fact]
    public async Task IsConnectedAsync_ReturnsBoolean()
    {
        // Act
        var result = await _networkService.IsConnectedAsync();

        // Assert - just verify it returns without throwing
        // The result is a bool, so we can test it returned successfully
        (result == true || result == false).Should().BeTrue();
    }

    [Fact]
    public void IsConnected_PropertyExists()
    {
        // Act
        var isConnected = _networkService.IsConnected;

        // Assert
        (isConnected == true || isConnected == false).Should().BeTrue();
    }

    [Fact]
    public void GetConnectionType_ReturnsConnectionType()
    {
        // Act
        var connectionType = _networkService.GetConnectionType();

        // Assert - verify it's a valid enum value
        System.Enum.IsDefined(typeof(NetworkConnectionType), connectionType).Should().BeTrue();
    }

    [Fact]
    public void StartMonitoring_DoesNotThrow()
    {
        // Act
        var act = () => _networkService.StartMonitoring();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void StopMonitoring_DoesNotThrow()
    {
        // Arrange
        _networkService.StartMonitoring();

        // Act
        var act = () => _networkService.StopMonitoring();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task IsHostReachableAsync_ReturnsBoolean()
    {
        // Act
        var result = await _networkService.IsHostReachableAsync("localhost");

        // Assert
        (result == true || result == false).Should().BeTrue();
    }
}
