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

public class GuideListViewModelTests
{
    private readonly Mock<IGuideRepository> _mockGuideRepository;
    private readonly GuideListViewModel _viewModel;

    public GuideListViewModelTests()
    {
        _mockGuideRepository = new Mock<IGuideRepository>();
        _viewModel = new GuideListViewModel(_mockGuideRepository.Object);
    }

    [Fact]
    public async Task LoadGuidesAsync_PopulatesGuides()
    {
        // Arrange
        var testGuides = new List<Guide>
        {
            new Guide { Id = 1, Title = "Guide 1", Category = "Category A" },
            new Guide { Id = 2, Title = "Guide 2", Category = "Category B" }
        };
        _mockGuideRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(testGuides);

        // Act
        await _viewModel.LoadGuidesAsync();

        // Assert
        _viewModel.Guides.Should().HaveCount(2);
        _viewModel.Guides.First().Title.Should().Be("Guide 1");
    }

    [Fact]
    public async Task SearchGuides_FiltersCorrectly()
    {
        // Arrange
        var testGuides = new List<Guide>
        {
            new Guide { Id = 1, Title = "Motor Installation", Category = "Mechanical" },
            new Guide { Id = 2, Title = "Sensor Calibration", Category = "Electrical" },
            new Guide { Id = 3, Title = "Motor Alignment", Category = "Mechanical" }
        };
        _mockGuideRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(testGuides);
        await _viewModel.LoadGuidesAsync();

        // Act
        _viewModel.SearchQuery = "Motor";

        // Assert
        _viewModel.FilteredGuides.Should().HaveCount(2);
        _viewModel.FilteredGuides.All(g => g.Title.Contains("Motor")).Should().BeTrue();
    }

    [Fact]
    public async Task FilterByCategory_FiltersCorrectly()
    {
        // Arrange
        var testGuides = new List<Guide>
        {
            new Guide { Id = 1, Title = "Guide 1", Category = "Mechanical" },
            new Guide { Id = 2, Title = "Guide 2", Category = "Electrical" },
            new Guide { Id = 3, Title = "Guide 3", Category = "Mechanical" }
        };
        _mockGuideRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(testGuides);
        await _viewModel.LoadGuidesAsync();

        // Act
        _viewModel.SelectedCategory = "Mechanical";

        // Assert
        _viewModel.FilteredGuides.Should().HaveCount(2);
        _viewModel.FilteredGuides.All(g => g.Category == "Mechanical").Should().BeTrue();
    }

    [Fact]
    public void ClearFilters_ResetsSearchAndCategory()
    {
        // Arrange
        _viewModel.SearchQuery = "test";
        _viewModel.SelectedCategory = "Mechanical";

        // Act
        _viewModel.ClearFiltersCommand.Execute(null);

        // Assert
        _viewModel.SearchQuery.Should().BeNullOrEmpty();
        _viewModel.SelectedCategory.Should().BeNull();
    }
}
