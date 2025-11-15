using Xunit;
using FluentAssertions;
using InstallVibe.Models;
using System.Collections.Generic;

namespace InstallVibe.Tests.Models;

public class GuideTests
{
    [Fact]
    public void Guide_DefaultConstructor_InitializesWithDefaults()
    {
        // Act
        var guide = new Guide();

        // Assert
        guide.Id.Should().Be(0);
        guide.Title.Should().Be(string.Empty);
        guide.Description.Should().Be(string.Empty);
        guide.Category.Should().Be(string.Empty);
        guide.Steps.Should().NotBeNull();
        guide.Steps.Should().BeEmpty();
    }

    [Fact]
    public void Guide_ParameterizedConstructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var guide = new Guide(
            id: 1,
            title: "Motor Installation",
            description: "Install motor assembly",
            category: "Mechanical",
            createdByUserId: 5
        );

        // Assert
        guide.Id.Should().Be(1);
        guide.Title.Should().Be("Motor Installation");
        guide.Description.Should().Be("Install motor assembly");
        guide.Category.Should().Be("Mechanical");
        guide.CreatedByUserId.Should().Be(5);
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
            EstimatedDurationMinutes = 45,
            CreatedByUserId = 5
        };

        // Assert
        guide.Id.Should().Be(1);
        guide.Title.Should().Be("Motor Installation");
        guide.Description.Should().Be("Install motor assembly");
        guide.Category.Should().Be("Mechanical");
        guide.EstimatedDurationMinutes.Should().Be(45);
        guide.CreatedByUserId.Should().Be(5);
    }

    [Fact]
    public void Guide_Steps_CanBeAdded()
    {
        // Arrange
        var guide = new Guide();
        var step = new Step
        {
            Id = 1,
            GuideId = guide.Id,
            StepNumber = 1,
            Title = "First Step",
            Instruction = "Do something"
        };

        // Act
        guide.Steps.Add(step);

        // Assert
        guide.Steps.Should().HaveCount(1);
        guide.Steps.Should().Contain(step);
    }

    [Fact]
    public void Step_DefaultConstructor_InitializesWithDefaults()
    {
        // Act
        var step = new Step();

        // Assert
        step.Id.Should().Be(0);
        step.StepNumber.Should().Be(0);
        step.Title.Should().Be(string.Empty);
        step.Instruction.Should().Be(string.Empty);
    }

    [Fact]
    public void Step_ParameterizedConstructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var step = new Step(
            id: 1,
            guideId: 5,
            stepNumber: 3,
            title: "Align motor shaft",
            instruction: "Use alignment tool to center shaft"
        );

        // Assert
        step.Id.Should().Be(1);
        step.GuideId.Should().Be(5);
        step.StepNumber.Should().Be(3);
        step.Title.Should().Be("Align motor shaft");
        step.Instruction.Should().Be("Use alignment tool to center shaft");
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
            Instruction = "Use alignment tool to center shaft",
            RequiredTools = "Wrench, Alignment tool",
            SafetyNotes = "Ensure power is disconnected"
        };

        // Assert
        step.Id.Should().Be(1);
        step.GuideId.Should().Be(5);
        step.StepNumber.Should().Be(3);
        step.Title.Should().Be("Align motor shaft");
        step.Instruction.Should().Be("Use alignment tool to center shaft");
        step.RequiredTools.Should().Be("Wrench, Alignment tool");
        step.SafetyNotes.Should().Be("Ensure power is disconnected");
    }

    [Fact]
    public void Step_Media_CanBeAdded()
    {
        // Arrange
        var step = new Step();
        var mediaItem = new MediaItem
        {
            Id = 1,
            StepId = step.Id,
            MediaType = MediaType.Image,
            FilePath = "images/step1.png"
        };

        // Act
        step.Media.Add(mediaItem);

        // Assert
        step.Media.Should().HaveCount(1);
        step.Media.Should().Contain(mediaItem);
    }

    [Fact]
    public void MediaItem_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var mediaItem = new MediaItem
        {
            Id = 1,
            StepId = 5,
            MediaType = MediaType.Video,
            FilePath = "videos/installation.mp4"
        };

        // Assert
        mediaItem.Id.Should().Be(1);
        mediaItem.StepId.Should().Be(5);
        mediaItem.MediaType.Should().Be(MediaType.Video);
        mediaItem.FilePath.Should().Be("videos/installation.mp4");
    }

    [Fact]
    public void MediaItem_ParameterizedConstructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var mediaItem = new MediaItem(
            id: 1,
            stepId: 5,
            mediaType: MediaType.Image,
            filePath: "images/diagram.png"
        );

        // Assert
        mediaItem.Id.Should().Be(1);
        mediaItem.StepId.Should().Be(5);
        mediaItem.MediaType.Should().Be(MediaType.Image);
        mediaItem.FilePath.Should().Be("images/diagram.png");
    }
}
