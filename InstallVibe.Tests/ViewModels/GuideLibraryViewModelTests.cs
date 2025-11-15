using Xunit;
using Moq;
using FluentAssertions;
using InstallVibe.ViewModels;
using InstallVibe.Models;
using InstallVibe.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstallVibe.Tests.ViewModels;

public class GuideLibraryViewModelTests
{
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly GuideLibraryViewModel _viewModel;

    public GuideLibraryViewModelTests()
    {
        _mockNavigationService = new Mock<INavigationService>();
        _mockAuthService = new Mock<IAuthService>();

        // Setup default auth service behavior
        _mockAuthService.Setup(x => x.GetCurrentUser()).Returns(new User
        {
            Id = 1,
            Username = "testuser",
            IsAdmin = false
        });
        _mockAuthService.Setup(x => x.IsAdmin()).Returns(false);

        _viewModel = new GuideLibraryViewModel(_mockNavigationService.Object, _mockAuthService.Object);
    }

    [Fact]
    public void Constructor_InitializesViewModel()
    {
        // Assert
        _viewModel.Should().NotBeNull();
        _viewModel.Guides.Should().BeEmpty();
        _viewModel.Categories.Should().BeEmpty();
        _viewModel.SearchText.Should().BeEmpty();
        _viewModel.SelectedCategory.Should().Be("All");
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void IsAdmin_ReturnsFalse_WhenUserIsNotAdmin()
    {
        // Arrange
        _mockAuthService.Setup(x => x.IsAdmin()).Returns(false);

        // Act
        var result = _viewModel.IsAdmin;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAdmin_ReturnsTrue_WhenUserIsAdmin()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(x => x.IsAdmin()).Returns(true);
        var viewModel = new GuideLibraryViewModel(_mockNavigationService.Object, mockAuthService.Object);

        // Act
        var result = viewModel.IsAdmin;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void NavigateToGuideCommand_CallsNavigationService()
    {
        // Arrange
        var guide = new GuideListItem { Id = 1, Title = "Test Guide" };

        // Act
        _viewModel.NavigateToGuideCommand.Execute(guide);

        // Assert
        _mockNavigationService.Verify(x => x.NavigateToGuideDetail(1), Times.Once);
    }

    [Fact]
    public void EditGuideCommand_CallsNavigationService()
    {
        // Arrange
        var guide = new GuideListItem { Id = 1, Title = "Test Guide" };

        // Act
        _viewModel.EditGuideCommand.Execute(guide);

        // Assert
        _mockNavigationService.Verify(x => x.NavigateToGuideEditor(1), Times.Once);
    }

    [Fact]
    public void GoBackCommand_CallsNavigationService()
    {
        // Act
        _viewModel.GoBackCommand.Execute(null);

        // Assert
        _mockNavigationService.Verify(x => x.GoBack(), Times.Once);
    }

    [Fact]
    public void SearchText_PropertyChanges_TriggersPropertyChanged()
    {
        // Arrange
        var propertyChangedFired = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(GuideLibraryViewModel.SearchText))
                propertyChangedFired = true;
        };

        // Act
        _viewModel.SearchText = "test";

        // Assert
        propertyChangedFired.Should().BeTrue();
        _viewModel.SearchText.Should().Be("test");
    }

    [Fact]
    public void SelectedCategory_PropertyChanges_TriggersPropertyChanged()
    {
        // Arrange
        var propertyChangedFired = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(GuideLibraryViewModel.SelectedCategory))
                propertyChangedFired = true;
        };

        // Act
        _viewModel.SelectedCategory = "Mechanical";

        // Assert
        propertyChangedFired.Should().BeTrue();
        _viewModel.SelectedCategory.Should().Be("Mechanical");
    }
}
