using Xunit;
using FluentAssertions;
using InstallVibe.Models;
using System.Collections.Generic;

namespace InstallVibe.Tests.Models;

public class GuideTests
{
    [Fact]
    public void Guide_InitializesWithDefaults()
    {
        // Act
        var guide = new Guide();

        // Assert
        guide.Id.Should().Be(0);
        guide.Title.Should().BeNull();
        guide.Description.Should().BeNull();
        guide.Steps.Should().BeNull();
    }

    [Fact]
    public void Guide_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var guide = new Guide
        {
            Id = 1,
            Title = "Motor Installation",
            Description = "Install motor assembly",
            Category = "Mechanical",
            EstimatedMinutes = 45,
            DifficultyLevel = "Intermediate",
            RequiredTools = new List<string> { "Wrench", "Screwdriver" },
            SafetyWarnings = new List<string> { "Wear gloves" }
        };

        // Assert
        guide.Id.Should().Be(1);
        guide.Title.Should().Be("Motor Installation");
        guide.Description.Should().Be("Install motor assembly");
        guide.Category.Should().Be("Mechanical");
        guide.EstimatedMinutes.Should().Be(45);
        guide.DifficultyLevel.Should().Be("Intermediate");
        guide.RequiredTools.Should().HaveCount(2);
        guide.SafetyWarnings.Should().HaveCount(1);
    }

    [Fact]
    public void Step_InitializesWithDefaults()
    {
        // Act
        var step = new Step();

        // Assert
        step.Id.Should().Be(0);
        step.StepNumber.Should().Be(0);
        step.Title.Should().BeNull();
        step.Description.Should().BeNull();
    }

    [Fact]
    public void Step_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var step = new Step
        {
            Id = 1,
            GuideId = 5,
            StepNumber = 3,
            Title = "Align motor shaft",
            Description = "Use alignment tool to center shaft",
            MediaType = "Image",
            MediaUrls = new List<string> { "step3-image1.png", "step3-image2.png" },
            EstimatedMinutes = 10,
            SafetyNote = "Ensure power is disconnected"
        };

        // Assert
        step.Id.Should().Be(1);
        step.GuideId.Should().Be(5);
        step.StepNumber.Should().Be(3);
        step.Title.Should().Be("Align motor shaft");
        step.Description.Should().Be("Use alignment tool to center shaft");
        step.MediaType.Should().Be("Image");
        step.MediaUrls.Should().HaveCount(2);
        step.EstimatedMinutes.Should().Be(10);
        step.SafetyNote.Should().Be("Ensure power is disconnected");
    }

    [Fact]
    public void Progress_TracksCompletion()
    {
        // Arrange & Act
        var progress = new Progress
        {
            Id = 1,
            GuideId = 5,
            UserId = "tech123",
            CurrentStepNumber = 3,
            CompletedSteps = new List<int> { 1, 2, 3 },
            IsCompleted = false,
            StartedAt = System.DateTime.UtcNow.AddMinutes(-30),
            LastUpdatedAt = System.DateTime.UtcNow
        };

        // Assert
        progress.GuideId.Should().Be(5);
        progress.UserId.Should().Be("tech123");
        progress.CurrentStepNumber.Should().Be(3);
        progress.CompletedSteps.Should().HaveCount(3);
        progress.IsCompleted.Should().BeFalse();
    }
}
