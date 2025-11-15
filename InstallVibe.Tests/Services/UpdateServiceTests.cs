using Xunit;
using Moq;
using FluentAssertions;
using InstallVibe.Services;
using System.Threading.Tasks;

namespace InstallVibe.Tests.Services;

public class UpdateServiceTests
{
    private readonly Mock<INetworkService> _mockNetworkService;
    private readonly UpdateService _updateService;

    public UpdateServiceTests()
    {
        _mockNetworkService = new Mock<INetworkService>();
        _updateService = new UpdateService(_mockNetworkService.Object);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WhenOffline_ReturnsFalse()
    {
        // Arrange
        _mockNetworkService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(false);

        // Act
        var result = await _updateService.CheckForUpdatesAsync();

        // Assert
        result.Should().BeFalse();
        _updateService.IsUpdateAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WhenOnline_ChecksForUpdates()
    {
        // Arrange
        _mockNetworkService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);

        // Act
        var result = await _updateService.CheckForUpdatesAsync();

        // Assert
        _mockNetworkService.Verify(x => x.IsConnectedAsync(), Times.Once);
    }

    [Fact]
    public void CurrentVersion_ReturnsValidVersion()
    {
        // Act
        var version = _updateService.CurrentVersion;

        // Assert
        version.Should().NotBeNull();
        version.Major.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData("1.0.0.0", "1.0.1.0", true)]
    [InlineData("1.0.0.0", "1.1.0.0", true)]
    [InlineData("1.0.0.0", "2.0.0.0", true)]
    [InlineData("1.1.0.0", "1.0.0.0", false)]
    [InlineData("1.0.0.0", "1.0.0.0", false)]
    public void CompareVersions_ReturnsExpectedResult(string current, string latest, bool expectedNewer)
    {
        // Arrange
        var currentVersion = new System.Version(current);
        var latestVersion = new System.Version(latest);

        // Act
        var isNewer = latestVersion > currentVersion;

        // Assert
        isNewer.Should().Be(expectedNewer);
    }
}
