# Step 5: Guide Viewer + Progress UI - Implementation Guide

## Overview

Step 5 implements a split-pane interactive guide viewer that allows technicians to:
- View all steps at once with completion status
- Mark steps complete/incomplete with optimistic UI updates
- Track progress with real-time percentage and step counts
- Navigate between steps with keyboard shortcuts
- Reset progress with confirmation dialog

## Files Created

### ViewModels
- `ViewModels/StepViewModel.cs` - Helper class wrapping Step model with UI properties
- `ViewModels/GuideViewerViewModel.cs` - Main ViewModel with all business logic

### Views
- `Views/GuideViewerView.xaml` - Split-pane XAML layout
- `Views/GuideViewerView.xaml.cs` - Code-behind with keyboard handling

### Converters
- `Converters/BoolToCompleteTextConverter.cs` - Converts boolean to "Mark Complete"/"Mark Incomplete"

## Repository Integration

The GuideViewerViewModel integrates with IGuideRepository using three key methods:

### 1. Load Guide and Progress

```csharp
// Load guide with all steps
_guide = await repository.GetGuide(_guideId);

// Load user's progress
var progress = await repository.GetProgress(_guideId, currentUser.Id);
_completedStepIds = progress?.CompletedStepIds ?? new List<int>();
```

### 2. Save Progress (Optimistic Update)

```csharp
// Update UI immediately (optimistic)
CurrentStep.IsCompleted = !CurrentStep.IsCompleted;

if (CurrentStep.IsCompleted)
{
    _completedStepIds.Add(CurrentStep.Id);
}
else
{
    _completedStepIds.Remove(CurrentStep.Id);
}

UpdateProgress(); // Update percentage and text

// Persist to database
try
{
    await repository.SaveProgress(_guideId, currentUser.Id, _completedStepIds);
}
catch (Exception ex)
{
    // Revert UI on error
    CurrentStep.IsCompleted = !CurrentStep.IsCompleted;
    // ... revert completion list
    ErrorMessage = "Failed to save progress. Please try again.";
}
```

### 3. Reset Progress

```csharp
// Clear all completion states
foreach (var step in Steps)
{
    step.IsCompleted = false;
}

_completedStepIds.Clear();
UpdateProgress();

// Persist empty progress
await repository.SaveProgress(_guideId, currentUser.Id, _completedStepIds);
```

## Repository Methods Required

Ensure your `IGuideRepository` and `GuideRepository` implement these methods:

```csharp
public interface IGuideRepository
{
    Task<Guide?> GetGuide(int guideId);
    Task<List<Guide>> GetAllGuides();
    Task<GuideProgress?> GetProgress(int guideId, int userId);
    Task SaveProgress(int guideId, int userId, List<int> completedStepIds);
}
```

### SaveProgress Implementation Example

```csharp
public async Task SaveProgress(int guideId, int userId, List<int> completedStepIds)
{
    var progress = await _context.GuideProgresses
        .FirstOrDefaultAsync(p => p.GuideId == guideId && p.UserId == userId);

    if (progress == null)
    {
        progress = new GuideProgress
        {
            GuideId = guideId,
            UserId = userId,
            CompletedStepIds = completedStepIds,
            LastUpdated = DateTime.UtcNow
        };
        _context.GuideProgresses.Add(progress);
    }
    else
    {
        progress.CompletedStepIds = completedStepIds;
        progress.LastUpdated = DateTime.UtcNow;
    }

    await _context.SaveChangesAsync();
}
```

## Persistence & Concurrency

### Optimistic Local Persistence

The implementation uses **optimistic updates** for better UX:

1. **Update UI first** - User sees immediate feedback
2. **Persist to database** - Async save in background
3. **Revert on error** - If save fails, revert UI and show error message

This provides a responsive experience even on slow networks.

### Handling Multiple Devices

For future multi-device sync, consider these strategies:

#### Option 1: Last-Writer-Wins (Simple)
```csharp
// Use LastUpdated timestamp to determine most recent
if (incomingProgress.LastUpdated > localProgress.LastUpdated)
{
    // Accept incoming changes
    localProgress = incomingProgress;
}
```

#### Option 2: Merge Completed Steps (Better UX)
```csharp
// Merge both completion lists - a step completed on any device stays completed
var mergedSteps = localProgress.CompletedStepIds
    .Union(incomingProgress.CompletedStepIds)
    .ToList();

progress.CompletedStepIds = mergedSteps;
progress.LastUpdated = DateTime.UtcNow;
```

#### Option 3: Conflict Resolution UI
```csharp
// Detect conflicts
if (HasConflict(localProgress, serverProgress))
{
    // Show dialog: "Your progress differs from the server. Keep local or use server version?"
    var choice = await ShowConflictDialog(localProgress, serverProgress);
    // Apply user's choice
}
```

**Recommendation**: Use Option 2 (merge) - it's the most user-friendly. Steps completed on any device remain completed.

## XAML Bindings Summary

| UI Element | Binding | Mode |
|-----------|---------|------|
| Steps List ItemsSource | `ViewModel.Steps` | OneWay |
| Current Step | `ViewModel.CurrentStep` | OneWay |
| Step Title | `CurrentStep.Title` | OneWay |
| Step Instruction | `CurrentStep.Instruction` | OneWay |
| Completion Icon | `StepViewModel.CompletionIcon` | OneWay |
| Progress Bar Value | `ViewModel.ProgressPercent` | OneWay |
| Progress Text | `ViewModel.ProgressText` | OneWay |
| Mark Complete Button | `MarkStepCompleteCommand` | Command |
| Previous/Next Buttons | `PreviousStepCommand` / `NextStepCommand` | Command |
| Reset Progress Button | `ResetProgressCommand` | Command |

## Accessibility Features

### Keyboard Shortcuts
- **Left Arrow** → Previous Step
- **Right Arrow** → Next Step
- **Space** → Toggle Complete Current Step

Implemented in `GuideViewerView.xaml.cs`:

```csharp
private void GuideViewerView_KeyDown(object sender, KeyRoutedEventArgs e)
{
    if (e.Key == VirtualKey.Left && ViewModel.CanGoPrevious)
    {
        ViewModel.PreviousStepCommand.Execute(null);
        e.Handled = true;
    }
    // ... other shortcuts
}
```

### Touch Support
- **Large tap targets** - Minimum 44x44px for step buttons
- **Padding** - 12-20px padding on interactive elements
- **Visual feedback** - Buttons show pressed state

### Screen Reader Support
- All buttons have accessible text
- Progress bar has value/max for screen readers
- Step numbers announced in order

## Visual & UX Details

### Animations

Subtle fade-in when switching steps:

```xml
<Storyboard x:Name="StepTransitionStoryboard">
    <DoubleAnimation
        Storyboard.TargetName="StepDetailPanel"
        Storyboard.TargetProperty="Opacity"
        From="0.0" To="1.0" Duration="0:0:0.3">
        <DoubleAnimation.EasingFunction>
            <CubicEase EasingMode="EaseOut" />
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
</Storyboard>
```

Triggered when selecting a step:

```csharp
if (StepTransitionStoryboard != null)
{
    StepTransitionStoryboard.Begin();
}
```

### Visual States

| State | Visual Treatment |
|-------|-----------------|
| Completed Step | ✓ Green checkmark icon |
| Incomplete Step | ○ Circle outline icon |
| Current/Selected Step | Highlighted in list (IsSelected property) |
| All Steps Complete | Success banner with "Finish" button |
| Loading | ProgressRing centered |
| Error | Red InfoBar with retry option |

## Test / Example Usage

### Basic Test Flow

```csharp
// 1. Create ViewModel with dependencies
var guideRepository = scope.ServiceProvider.GetRequiredService<IGuideRepository>();
var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
var navService = scope.ServiceProvider.GetRequiredService<INavigationService>();

var vm = new GuideViewerViewModel(navService, authService);

// 2. Initialize with guide ID
var parameters = new Dictionary<string, object> { { "GuideId", 1 } };
vm.Initialize(parameters);

// Wait for async load
await Task.Delay(500);

// 3. Verify steps loaded
Assert.IsTrue(vm.Steps.Count > 0);
Assert.IsNotNull(vm.CurrentStep);
Assert.AreEqual(0, vm.ProgressPercent); // Assuming fresh start

// 4. Mark first step complete
await vm.MarkStepCompleteCommand.ExecuteAsync(null);

// 5. Verify progress updated
Assert.IsTrue(vm.Steps[0].IsCompleted);
Assert.IsTrue(vm.ProgressPercent > 0);
Assert.AreEqual("1 / 3 steps", vm.ProgressText); // Assuming 3 total steps

// 6. Verify persisted to database
var progress = await guideRepository.GetProgress(1, authService.GetCurrentUser().Id);
Assert.IsNotNull(progress);
Assert.IsTrue(progress.CompletedStepIds.Contains(vm.CurrentStep.Id));

// 7. Navigate to next step
vm.NextStepCommand.Execute(null);
Assert.AreEqual(1, vm.CurrentStep.Index);

// 8. Reset progress
await vm.ResetProgressCommand.ExecuteAsync(null);
Assert.AreEqual(0, vm.ProgressPercent);
Assert.IsTrue(vm.Steps.All(s => !s.IsCompleted));
```

### Integration Test

```csharp
[TestMethod]
public async Task TestCompleteGuideFlow()
{
    // Login as admin
    var authService = App.Services.GetRequiredService<IAuthService>();
    await authService.Login("admin", "admin123");

    // Navigate to guide viewer
    var navService = App.Services.GetRequiredService<INavigationService>();
    navService.NavigateToGuideViewer(1); // HVAC guide

    // Get ViewModel from current page
    var vm = (navService.Frame.Content as GuideViewerView)?.ViewModel;
    Assert.IsNotNull(vm);

    // Complete all steps
    for (int i = 0; i < vm.Steps.Count; i++)
    {
        vm.SelectStepCommand.Execute(vm.Steps[i]);
        await vm.MarkStepCompleteCommand.ExecuteAsync(null);
    }

    // Verify 100% complete
    Assert.AreEqual(100.0, vm.ProgressPercent);
    Assert.IsTrue(vm.IsAllCompleted);

    // Verify in database
    var repository = App.Services.GetRequiredService<IGuideRepository>();
    var progress = await repository.GetProgress(1, authService.GetCurrentUser().Id);
    Assert.AreEqual(vm.Steps.Count, progress.CompletedStepIds.Count);
}
```

## Navigation Integration

### From Guide Detail View

Add a button to navigate to the split-pane viewer:

```xml
<Button
    Content="View in Split-Pane Mode"
    Command="{x:Bind ViewModel.OpenInGuideViewerCommand}"
    Style="{StaticResource AccentButtonStyle}" />
```

ViewModel:

```csharp
[RelayCommand]
private void OpenInGuideViewer()
{
    _navigationService.NavigateToGuideViewer(_guideId);
}
```

### From Guide Library

Already implemented via existing "View Guide" buttons that navigate to GuideDetailView, but you could add a direct link:

```csharp
_navigationService.NavigateToGuideViewer(guide.Id);
```

## ✅ Acceptance Checklist

- ✅ **GuideViewerView.xaml** created with left steps list and right detail pane
- ✅ **GuideViewerViewModel.cs** implemented with all commands and properties:
  - ✅ `ObservableCollection<StepViewModel> Steps`
  - ✅ `StepViewModel CurrentStep`
  - ✅ `MarkStepCompleteCommand`
  - ✅ `NextStepCommand` / `PreviousStepCommand`
  - ✅ `ResetProgressCommand`
  - ✅ `ProgressPercent` and `ProgressText` properties
- ✅ **StepViewModel.cs** helper implemented with:
  - ✅ `IsCompleted` property
  - ✅ `CompletionIcon` computed property
  - ✅ `Index` and presentation properties
- ✅ **XAML bindings** wired to ViewModel commands and progress properties
- ✅ **Progress persists** via `GuideRepository.SaveProgress` and restores correctly
- ✅ **UI updates immediately** when marking steps complete (optimistic update)
- ✅ **Reset progress** functionality implemented with confirmation dialog
- ✅ **Accessibility features** present:
  - ✅ Keyboard shortcuts (Left/Right arrows, Space)
  - ✅ Large touch targets (44x44px minimum)
  - ✅ Screen reader support (accessible labels)
- ✅ **Animations** present (subtle fade transition between steps)
- ✅ **Registered in DI** container (App.xaml.cs)
- ✅ **Converter registered** in App.xaml
- ✅ **Navigation method** added to INavigationService

## Key Differences from StepViewerView

The codebase now has TWO guide viewing options:

| Feature | StepViewerView (Existing) | GuideViewerView (New) |
|---------|---------------------------|----------------------|
| Layout | Single step fullscreen | Split-pane: list + detail |
| Navigation | Prev/Next buttons only | Click any step in list |
| Progress View | Footer bar | Footer bar + list checkmarks |
| Use Case | Linear step-through | Jump between steps, overview |
| Keyboard | Basic arrows | Arrows + selection |

Both views are valuable - StepViewerView for focused work, GuideViewerView for overview and jumping around.

## Troubleshooting

### Issue: Steps Not Loading

**Check:**
1. GuideId parameter passed correctly
2. Database has guide with steps
3. User is authenticated
4. Repository GetGuide() returns data with eager-loaded Steps

**Debug:**
```csharp
System.Diagnostics.Debug.WriteLine($"Guide ID: {_guideId}");
System.Diagnostics.Debug.WriteLine($"Guide: {_guide?.Title}");
System.Diagnostics.Debug.WriteLine($"Steps Count: {_guide?.Steps.Count}");
```

### Issue: Progress Not Saving

**Check:**
1. User has valid ID
2. Database connection working
3. SaveProgress throws no exceptions
4. CompletedStepIds JSON serialization working

**Debug:**
```csharp
try
{
    await repository.SaveProgress(_guideId, userId, _completedStepIds);
    Debug.WriteLine("Save successful");
}
catch (Exception ex)
{
    Debug.WriteLine($"Save failed: {ex.Message}");
    Debug.WriteLine($"Stack: {ex.StackTrace}");
}
```

### Issue: Keyboard Shortcuts Not Working

**Check:**
1. Page has focus
2. KeyDown event handler attached
3. e.Handled = true to prevent bubbling

**Fix:**
```csharp
// In code-behind constructor
KeyDown += GuideViewerView_KeyDown;

// Ensure page is focusable
this.IsTabStop = true;
```

## Next Steps

Possible enhancements for future iterations:

1. **Media Support** - Display images/videos in step detail
2. **Notes Field** - Allow technicians to add notes per step
3. **Photo Capture** - Take photos during installation
4. **Timer** - Track time spent on each step
5. **Offline Sync** - Queue progress updates when offline
6. **Print View** - Generate PDF of guide
7. **Share Progress** - Send progress report via email
8. **Multiple Users** - Show who completed which steps

## Summary

Step 5 provides a complete, production-ready guide viewer with:
- ✅ Split-pane UI showing all steps at once
- ✅ Real-time progress tracking and persistence
- ✅ Optimistic updates for responsive UX
- ✅ Keyboard and touch accessibility
- ✅ Smooth animations
- ✅ Error handling with retry
- ✅ Confirmation dialogs for destructive actions
- ✅ Full test coverage examples

The implementation follows MVVM best practices, integrates cleanly with existing code, and provides a solid foundation for future enhancements.
