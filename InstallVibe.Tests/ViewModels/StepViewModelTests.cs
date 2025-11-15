using Xunit;
using Moq;
using FluentAssertions;
using InstallVibe.ViewModels;
using InstallVibe.Models;
using InstallVibe.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstallVibe.Tests.ViewModels;

public class StepViewModelTests
{
    private readonly Mock<IGuideRepository> _mockGuideRepository;
    private readonly Mock<IProgressService> _mockProgressService;
    private readonly StepViewModel _viewModel;

    public StepViewModelTests()
    {
        _mockGuideRepository = new Mock<IGuideRepository>();
        _mockProgressService = new Mock<IProgressService>();
        _viewModel = new StepViewModel(_mockGuideRepository.Object, _mockProgressService.Object);
    }

    [Fact]
    public async Task LoadStep_PopulatesStepData()
    {
        // Arrange
        var testStep = new Step
        {
            Id = 1,
            StepNumber = 1,
            Title = "Test Step",
            Description = "Test Description",
            MediaUrls = new List<string> { "image1.png" }
        };
        _mockGuideRepository.Setup(x => x.GetStepAsync(1, 1)).ReturnsAsync(testStep);

        // Act
        await _viewModel.LoadStepAsync(1, 1);

        // Assert
        _viewModel.CurrentStep.Should().NotBeNull();
        _viewModel.CurrentStep.Title.Should().Be("Test Step");
        _viewModel.StepNumber.Should().Be(1);
    }

    [Fact]
    public async Task MarkStepComplete_UpdatesProgress()
    {
        // Arrange
        var testStep = new Step { Id = 1, StepNumber = 1, Title = "Test", Description = "Test" };
        _mockGuideRepository.Setup(x => x.GetStepAsync(1, 1)).ReturnsAsync(testStep);
        await _viewModel.LoadStepAsync(1, 1);

        // Act
        await _viewModel.MarkStepCompleteAsync();

        // Assert
        _mockProgressService.Verify(x => x.MarkStepCompleteAsync(1, 1), Times.Once);
    }

    [Fact]
    public async Task NavigateNext_AdvancesToNextStep()
    {
        // Arrange
        var step1 = new Step { Id = 1, StepNumber = 1, Title = "Step 1", Description = "Test" };
        var step2 = new Step { Id = 2, StepNumber = 2, Title = "Step 2", Description = "Test" };
        _mockGuideRepository.Setup(x => x.GetStepAsync(1, 1)).ReturnsAsync(step1);
        _mockGuideRepository.Setup(x => x.GetStepAsync(1, 2)).ReturnsAsync(step2);
        _mockGuideRepository.Setup(x => x.GetTotalStepsAsync(1)).ReturnsAsync(5);
        await _viewModel.LoadStepAsync(1, 1);

        // Act
        await _viewModel.NextStepCommand.ExecuteAsync(null);

        // Assert
        _viewModel.StepNumber.Should().Be(2);
    }

    [Fact]
    public async Task NavigatePrevious_GoesToPreviousStep()
    {
        // Arrange
        var step1 = new Step { Id = 1, StepNumber = 1, Title = "Step 1", Description = "Test" };
        var step2 = new Step { Id = 2, StepNumber = 2, Title = "Step 2", Description = "Test" };
        _mockGuideRepository.Setup(x => x.GetStepAsync(1, 1)).ReturnsAsync(step1);
        _mockGuideRepository.Setup(x => x.GetStepAsync(1, 2)).ReturnsAsync(step2);
        await _viewModel.LoadStepAsync(1, 2);

        // Act
        await _viewModel.PreviousStepCommand.ExecuteAsync(null);

        // Assert
        _viewModel.StepNumber.Should().Be(1);
    }

    [Theory]
    [InlineData(1, 5, true, false)]  // First step: can go next, can't go previous
    [InlineData(3, 5, true, true)]   // Middle step: can go both
    [InlineData(5, 5, false, true)]  // Last step: can't go next, can go previous
    public async Task NavigationCommands_HaveCorrectCanExecute(int currentStep, int totalSteps, bool canNext, bool canPrevious)
    {
        // Arrange
        var step = new Step { Id = currentStep, StepNumber = currentStep, Title = "Test", Description = "Test" };
        _mockGuideRepository.Setup(x => x.GetStepAsync(1, currentStep)).ReturnsAsync(step);
        _mockGuideRepository.Setup(x => x.GetTotalStepsAsync(1)).ReturnsAsync(totalSteps);
        await _viewModel.LoadStepAsync(1, currentStep);

        // Act & Assert
        _viewModel.NextStepCommand.CanExecute(null).Should().Be(canNext);
        _viewModel.PreviousStepCommand.CanExecute(null).Should().Be(canPrevious);
    }
}
