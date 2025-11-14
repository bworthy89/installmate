# Step 3: Database Models & Persistence Layer - Test Snippets

## Repository Test Examples

### 1. Get All Guides

```csharp
using InstallVibe.Services;
using Microsoft.Extensions.DependencyInjection;

// Get repository from DI container
using (var scope = App.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IGuideRepository>();

    var allGuides = await repo.GetAllGuides();

    foreach (var guide in allGuides)
    {
        Console.WriteLine($"Guide: {guide.Title}");
        Console.WriteLine($"  Category: {guide.Category}");
        Console.WriteLine($"  Steps: {guide.Steps.Count}");
        Console.WriteLine($"  Created by: {guide.CreatedByUser.Username}");
    }
}

// Expected output:
// Guide: Standard HVAC Unit Installation
//   Category: HVAC
//   Steps: 3
//   Created by: admin
```

### 2. Get Specific Guide with Steps and Media

```csharp
using (var scope = App.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IGuideRepository>();

    var guide = await repo.GetGuide(1); // Get HVAC guide

    if (guide != null)
    {
        Console.WriteLine($"Title: {guide.Title}");
        Console.WriteLine($"Description: {guide.Description}");
        Console.WriteLine($"Est. Duration: {guide.EstimatedDurationMinutes} minutes");
        Console.WriteLine($"\nSteps:");

        foreach (var step in guide.Steps.OrderBy(s => s.StepNumber))
        {
            Console.WriteLine($"\n  Step {step.StepNumber}: {step.Title}");
            Console.WriteLine($"    {step.Instruction}");
            Console.WriteLine($"    Tools: {step.RequiredTools}");
            Console.WriteLine($"    Safety: {step.SafetyNotes}");
            Console.WriteLine($"    Media items: {step.Media.Count}");
        }
    }
}

// Expected output:
// Title: Standard HVAC Unit Installation
// Description: Complete installation guide...
// Est. Duration: 180 minutes
//
// Steps:
//
//   Step 1: Pre-installation Safety Check
//     Before beginning installation...
//     Tools: Voltage tester, Safety glasses...
//     Safety: DANGER: Always verify power...
//     Media items: 1
//   ...
```

### 3. Create a New Guide

```csharp
using InstallVibe.Models;

using (var scope = App.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IGuideRepository>();
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

    var currentUser = authService.GetCurrentUser();

    var newGuide = new Guide
    {
        Title = "Water Heater Replacement",
        Description = "Step-by-step guide for replacing a residential water heater",
        Category = "Plumbing",
        CreatedByUserId = currentUser!.Id,
        EstimatedDurationMinutes = 120
    };

    // Add steps
    newGuide.Steps.Add(new Step
    {
        StepNumber = 1,
        Title = "Turn off water supply",
        Instruction = "Locate the main water shutoff valve and turn it clockwise to close.",
        RequiredTools = "None",
        SafetyNotes = "Ensure water is completely off before proceeding."
    });

    newGuide.Steps.Add(new Step
    {
        StepNumber = 2,
        Title = "Drain the tank",
        Instruction = "Connect a garden hose to the drain valve and run it to a floor drain.",
        RequiredTools = "Garden hose",
        SafetyNotes = "Water may be hot. Wait for tank to cool if necessary."
    });

    int newGuideId = await repo.CreateGuide(newGuide);

    Console.WriteLine($"Created guide with ID: {newGuideId}");
    Console.WriteLine($"Title: {newGuide.Title}");
    Console.WriteLine($"Steps: {newGuide.Steps.Count}");
}

// Expected output:
// Created guide with ID: 2
// Title: Water Heater Replacement
// Steps: 2
```

### 4. Save Progress for a User

```csharp
using (var scope = App.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IGuideRepository>();
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

    var currentUser = authService.GetCurrentUser();
    int guideId = 1; // HVAC guide

    // Mark steps 1 and 2 as completed
    var completedSteps = new List<int> { 1, 2 };

    await repo.SaveProgress(guideId, currentUser!.Id, completedSteps);

    Console.WriteLine($"Saved progress for user {currentUser.Username}");
    Console.WriteLine($"Guide ID: {guideId}");
    Console.WriteLine($"Completed steps: {string.Join(", ", completedSteps)}");
}

// Expected output:
// Saved progress for user admin
// Guide ID: 1
// Completed steps: 1, 2
```

### 5. Get User Progress

```csharp
using (var scope = App.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IGuideRepository>();
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

    var currentUser = authService.GetCurrentUser();
    int guideId = 1; // HVAC guide

    var progress = await repo.GetProgress(guideId, currentUser!.Id);

    if (progress != null)
    {
        Console.WriteLine($"Progress for: {progress.User.Username}");
        Console.WriteLine($"Guide: {progress.Guide.Title}");
        Console.WriteLine($"Completed steps: {string.Join(", ", progress.CompletedStepIds)}");
        Console.WriteLine($"Last updated: {progress.LastUpdated:yyyy-MM-dd HH:mm:ss}");

        // Calculate completion percentage
        var guide = await repo.GetGuide(guideId);
        if (guide != null)
        {
            double percentage = (progress.CompletedStepIds.Count / (double)guide.Steps.Count) * 100;
            Console.WriteLine($"Completion: {percentage:F1}%");
        }
    }
    else
    {
        Console.WriteLine("No progress found for this user/guide combination");
    }
}

// Expected output:
// Progress for: admin
// Guide: Standard HVAC Unit Installation
// Completed steps: 1, 2
// Last updated: 2024-01-15 14:30:45
// Completion: 66.7%
```

### 6. Update Guide

```csharp
using (var scope = App.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IGuideRepository>();

    var guide = await repo.GetGuide(1);

    if (guide != null)
    {
        // Update guide properties
        guide.EstimatedDurationMinutes = 210; // Increase time estimate
        guide.Description = guide.Description + " Updated with latest safety standards.";

        await repo.UpdateGuide(guide);

        Console.WriteLine("Guide updated successfully");
        Console.WriteLine($"New duration: {guide.EstimatedDurationMinutes} minutes");
    }
}

// Expected output:
// Guide updated successfully
// New duration: 210 minutes
```

### 7. Test Cascade Delete (Steps and Media)

```csharp
using InstallVibe.Data;
using Microsoft.EntityFrameworkCore;

using (var scope = App.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InstallVibeDbContext>();

    // Create a test guide with steps and media
    var testGuide = new Guide
    {
        Title = "Test Guide for Deletion",
        Description = "This guide will be deleted",
        Category = "Test",
        CreatedByUserId = 1
    };

    testGuide.Steps.Add(new Step
    {
        StepNumber = 1,
        Title = "Test Step",
        Instruction = "Test instruction",
        Media = new List<MediaItem>
        {
            new MediaItem
            {
                MediaType = MediaType.Image,
                FilePath = "/test/image.jpg"
            }
        }
    });

    context.Guides.Add(testGuide);
    await context.SaveChangesAsync();

    int guideId = testGuide.Id;

    Console.WriteLine($"Created test guide with ID: {guideId}");

    // Count related records
    var stepCount = await context.Steps.CountAsync(s => s.GuideId == guideId);
    var mediaCount = await context.MediaItems
        .CountAsync(m => context.Steps.Any(s => s.Id == m.StepId && s.GuideId == guideId));

    Console.WriteLine($"Steps: {stepCount}, Media: {mediaCount}");

    // Delete the guide
    context.Guides.Remove(testGuide);
    await context.SaveChangesAsync();

    Console.WriteLine("Guide deleted");

    // Verify cascade delete
    stepCount = await context.Steps.CountAsync(s => s.GuideId == guideId);
    mediaCount = await context.MediaItems
        .CountAsync(m => context.Steps.Any(s => s.Id == m.StepId && s.GuideId == guideId));

    Console.WriteLine($"After delete - Steps: {stepCount}, Media: {mediaCount}");
}

// Expected output:
// Created test guide with ID: 3
// Steps: 1, Media: 1
// Guide deleted
// After delete - Steps: 0, Media: 0
```

### 8. Test JSON Conversion for CompletedStepIds

```csharp
using InstallVibe.Data;

using (var scope = App.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InstallVibeDbContext>();

    // Create progress with multiple completed steps
    var progress = new GuideProgress
    {
        GuideId = 1,
        UserId = 1,
        CompletedStepIds = new List<int> { 1, 2, 3, 5, 7, 9 },
        LastUpdated = DateTime.UtcNow
    };

    context.GuideProgresses.Add(progress);
    await context.SaveChangesAsync();

    int progressId = progress.Id;

    // Clear the context to force re-read from database
    context.ChangeTracker.Clear();

    // Read back from database
    var reloadedProgress = await context.GuideProgresses.FindAsync(progressId);

    if (reloadedProgress != null)
    {
        Console.WriteLine("JSON Conversion Test:");
        Console.WriteLine($"Original: {string.Join(", ", progress.CompletedStepIds)}");
        Console.WriteLine($"Reloaded: {string.Join(", ", reloadedProgress.CompletedStepIds)}");
        Console.WriteLine($"Match: {progress.CompletedStepIds.SequenceEqual(reloadedProgress.CompletedStepIds)}");
    }

    // Clean up
    context.GuideProgresses.Remove(reloadedProgress!);
    await context.SaveChangesAsync();
}

// Expected output:
// JSON Conversion Test:
// Original: 1, 2, 3, 5, 7, 9
// Reloaded: 1, 2, 3, 5, 7, 9
// Match: True
```

## Running Tests in Visual Studio

### Option 1: Immediate Window (During Debug)

1. Set a breakpoint in `HomeViewModel` constructor after user logs in
2. Run the app (F5) and login
3. Open **Debug → Windows → Immediate**
4. Paste test code from above

### Option 2: Add Test Button to HomeView

Add to `HomeViewModel.cs`:

```csharp
[RelayCommand]
private async Task RunDatabaseTests()
{
    using (var scope = App.Services.CreateScope())
    {
        var repo = scope.ServiceProvider.GetRequiredService<IGuideRepository>();

        // Test 1: Get all guides
        var guides = await repo.GetAllGuides();
        System.Diagnostics.Debug.WriteLine($"Total guides: {guides.Count()}");

        // Test 2: Get specific guide
        var guide = await repo.GetGuide(1);
        System.Diagnostics.Debug.WriteLine($"Guide: {guide?.Title}, Steps: {guide?.Steps.Count}");

        // Test 3: Save progress
        var currentUser = _authService.GetCurrentUser();
        if (currentUser != null)
        {
            await repo.SaveProgress(1, currentUser.Id, new List<int> { 1, 2 });
            System.Diagnostics.Debug.WriteLine("Progress saved");

            // Test 4: Get progress
            var progress = await repo.GetProgress(1, currentUser.Id);
            System.Diagnostics.Debug.WriteLine($"Completed steps: {string.Join(", ", progress?.CompletedStepIds ?? new List<int>())}");
        }
    }
}
```

Add button to `HomeView.xaml`:

```xml
<Button Content="Run DB Tests" Command="{x:Bind ViewModel.RunDatabaseTestsCommand}" Margin="8"/>
```

Check the **Output** window in Visual Studio for debug output.

## Expected Test Results

✅ All guides retrieved with steps and media loaded
✅ Specific guide retrieved with ordered steps
✅ New guide created with auto-generated ID
✅ Progress saved and retrieved correctly
✅ CompletedStepIds persisted as JSON and deserialized correctly
✅ Guide update modifies database
✅ Cascade delete removes steps and media when guide is deleted
✅ EF Core tracking and change detection working
