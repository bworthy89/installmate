using Xunit;
using FluentAssertions;
using InstallVibe.ViewModels;
using InstallVibe.Models;

namespace InstallVibe.Tests.ViewModels;

public class StepViewModelTests
{
    [Fact]
    public void Constructor_InitializesViewModel()
    {
        // Arrange
        var step = new Step
        {
            Id = 1,
            StepNumber = 1,
            Title = "Test Step",
            Instruction = "Test instruction",
            RequiredTools = "Wrench, Screwdriver",
            SafetyNotes = "Wear safety goggles"
        };

        // Act
        var viewModel = new StepViewModel(step, 0, false);

        // Assert
        viewModel.Id.Should().Be(1);
        viewModel.StepNumber.Should().Be(1);
        viewModel.Title.Should().Be("Test Step");
        viewModel.Instruction.Should().Be("Test instruction");
        viewModel.RequiredTools.Should().Be("Wrench, Screwdriver");
        viewModel.SafetyNotes.Should().Be("Wear safety goggles");
        viewModel.Index.Should().Be(0);
        viewModel.IsCompleted.Should().BeFalse();
        viewModel.IsSelected.Should().BeFalse();
    }

    [Fact]
    public void StepNumberText_ReturnsFormattedText()
    {
        // Arrange
        var step = new Step { Id = 1, StepNumber = 5, Title = "Test", Instruction = "Test" };
        var viewModel = new StepViewModel(step, 4, false);

        // Act
        var result = viewModel.StepNumberText;

        // Assert
        result.Should().Be("Step 5");
    }

    [Fact]
    public void CompletionIcon_ShowsCircle_WhenNotCompleted()
    {
        // Arrange
        var step = new Step { Id = 1, StepNumber = 1, Title = "Test", Instruction = "Test" };
        var viewModel = new StepViewModel(step, 0, false);

        // Act
        var icon = viewModel.CompletionIcon;

        // Assert
        icon.Should().Be("\uE739"); // Circle
        viewModel.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void CompletionIcon_ShowsCheckMark_WhenCompleted()
    {
        // Arrange
        var step = new Step { Id = 1, StepNumber = 1, Title = "Test", Instruction = "Test" };
        var viewModel = new StepViewModel(step, 0, true);

        // Act
        var icon = viewModel.CompletionIcon;

        // Assert
        icon.Should().Be("\uE73E"); // CheckMark
        viewModel.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void IsCompleted_CanBeChanged()
    {
        // Arrange
        var step = new Step { Id = 1, StepNumber = 1, Title = "Test", Instruction = "Test" };
        var viewModel = new StepViewModel(step, 0, false);

        // Act
        viewModel.IsCompleted = true;

        // Assert
        viewModel.IsCompleted.Should().BeTrue();
        viewModel.CompletionIcon.Should().Be("\uE73E"); // CheckMark
    }

    [Fact]
    public void IsCompleted_PropertyChange_UpdatesCompletionIcon()
    {
        // Arrange
        var step = new Step { Id = 1, StepNumber = 1, Title = "Test", Instruction = "Test" };
        var viewModel = new StepViewModel(step, 0, false);
        var propertyChangedEvents = new System.Collections.Generic.List<string>();
        viewModel.PropertyChanged += (s, e) => propertyChangedEvents.Add(e.PropertyName!);

        // Act
        viewModel.IsCompleted = true;

        // Assert
        propertyChangedEvents.Should().Contain("IsCompleted");
        propertyChangedEvents.Should().Contain("CompletionIcon");
    }

    [Fact]
    public void IsSelected_CanBeChanged()
    {
        // Arrange
        var step = new Step { Id = 1, StepNumber = 1, Title = "Test", Instruction = "Test" };
        var viewModel = new StepViewModel(step, 0, false);

        // Act
        viewModel.IsSelected = true;

        // Assert
        viewModel.IsSelected.Should().BeTrue();
    }

    [Fact]
    public void Step_ReturnsUnderlyingStepModel()
    {
        // Arrange
        var step = new Step
        {
            Id = 1,
            StepNumber = 1,
            Title = "Test Step",
            Instruction = "Test instruction"
        };
        var viewModel = new StepViewModel(step, 0, false);

        // Act
        var result = viewModel.Step;

        // Assert
        result.Should().BeSameAs(step);
    }
}
