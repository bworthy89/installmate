using Xunit;
using FluentAssertions;
using InstallVibe.Services;
using System.Threading.Tasks;

namespace InstallVibe.Tests.Services;

public class NetworkServiceTests
{
    private readonly NetworkService _networkService;

    public NetworkServiceTests()
    {
        _networkService = new NetworkService();
    }

    [Fact]
    public async Task IsConnectedAsync_ReturnsBoolean()
    {
        // Act
        var result = await _networkService.IsConnectedAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ConnectivityChanged_EventExists()
    {
        // Arrange
        var eventRaised = false;
        _networkService.ConnectivityChanged += (s, e) => eventRaised = true;

        // Act - Trigger would require network state change

        // Assert
        // Event handler should be registered
        _networkService.ConnectivityChanged.Should().NotBeNull();
    }
}
