using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Models;
using InstallVibe.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InstallVibe.ViewModels;

public partial class StepViewerViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private int _guideId;
    private List<Step> _allSteps = new();
    private List<int> _completedStepIds = new();

    [ObservableProperty]
    private int _currentStepIndex = 0;

    [ObservableProperty]
    private int _stepNumber = 1;

    [ObservableProperty]
    private string _stepTitle = string.Empty;

    [ObservableProperty]
    private string _instruction = string.Empty;

    [ObservableProperty]
    private string? _requiredTools;

    [ObservableProperty]
    private string? _safetyNotes;

    [ObservableProperty]
    private ObservableCollection<MediaItemDisplay> _mediaItems = new();

    [ObservableProperty]
    private bool _isCompleted = false;

    [ObservableProperty]
    private string _progressText = "Step 1 of 1";

    [ObservableProperty]
    private double _progressPercentage = 0;

    [ObservableProperty]
    private bool _canGoPrevious = false;

    [ObservableProperty]
    private bool _canGoNext = false;

    [ObservableProperty]
    private string _completeButtonText = "Mark Step Complete";

    [ObservableProperty]
    private bool _isLoading = false;

    public StepViewerViewModel(INavigationService navigationService, IAuthService authService)
    {
        _navigationService = navigationService;
        _authService = authService;
    }

    public async Task InitializeAsync(int guideId, int stepIndex)
    {
        _guideId = guideId;
        _currentStepIndex = stepIndex;
        await LoadStepsAsync();
    }

    public void Initialize(Dictionary<string, object> parameters)
    {
        if (parameters.TryGetValue("GuideId", out var guideIdObj) && guideIdObj is int guideId)
        {
            var stepIndex = 0;
            if (parameters.TryGetValue("StepIndex", out var stepIndexObj) && stepIndexObj is int idx)
            {
                stepIndex = idx;
            }

            _ = InitializeAsync(guideId, stepIndex);
        }
    }

    private async Task LoadStepsAsync()
    {
        IsLoading = true;

        try
        {
            using var scope = App.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IGuideRepository>();
            var currentUser = _authService.GetCurrentUser();

            if (currentUser == null)
            {
                IsLoading = false;
                return;
            }

            // Load guide with steps
            var guide = await repository.GetGuide(_guideId);
            if (guide == null)
            {
                IsLoading = false;
                return;
            }

            _allSteps = guide.Steps.OrderBy(s => s.StepNumber).ToList();

            // Load progress
            var progress = await repository.GetProgress(_guideId, currentUser.Id);
            _completedStepIds = progress?.CompletedStepIds ?? new List<int>();

            // Display current step
            DisplayCurrentStep();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading steps: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void DisplayCurrentStep()
    {
        if (_currentStepIndex < 0 || _currentStepIndex >= _allSteps.Count)
        {
            return;
        }

        var step = _allSteps[_currentStepIndex];

        StepNumber = step.StepNumber;
        StepTitle = step.Title;
        Instruction = step.Instruction;
        RequiredTools = step.RequiredTools;
        SafetyNotes = step.SafetyNotes;

        // Load media
        var mediaDisplayItems = step.Media.Select(m => new MediaItemDisplay
        {
            Id = m.Id,
            FilePath = m.FilePath,
            MediaType = m.MediaType,
            TypeIcon = m.MediaType == MediaType.Image ? "🖼️" : "🎥"
        }).ToList();
        MediaItems = new ObservableCollection<MediaItemDisplay>(mediaDisplayItems);

        // Check if completed
        IsCompleted = _completedStepIds.Contains(step.Id);
        CompleteButtonText = IsCompleted ? "✓ Step Completed" : "Mark Step Complete";

        // Update progress
        ProgressText = $"Step {_currentStepIndex + 1} of {_allSteps.Count}";
        ProgressPercentage = (_currentStepIndex + 1) / (double)_allSteps.Count * 100;

        // Update navigation buttons
        CanGoPrevious = _currentStepIndex > 0;
        CanGoNext = _currentStepIndex < _allSteps.Count - 1;
    }

    [RelayCommand]
    private async Task ToggleStepComplete()
    {
        if (_currentStepIndex < 0 || _currentStepIndex >= _allSteps.Count)
        {
            return;
        }

        var currentStep = _allSteps[_currentStepIndex];
        var currentUser = _authService.GetCurrentUser();

        if (currentUser == null)
        {
            return;
        }

        try
        {
            // Toggle completion
            if (IsCompleted)
            {
                _completedStepIds.Remove(currentStep.Id);
            }
            else
            {
                if (!_completedStepIds.Contains(currentStep.Id))
                {
                    _completedStepIds.Add(currentStep.Id);
                }
            }

            // Save progress
            using var scope = App.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IGuideRepository>();
            await repository.SaveProgress(_guideId, currentUser.Id, _completedStepIds);

            // Update UI
            IsCompleted = !IsCompleted;
            CompleteButtonText = IsCompleted ? "✓ Step Completed" : "Mark Step Complete";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving progress: {ex.Message}");
        }
    }

    [RelayCommand]
    private void PreviousStep()
    {
        if (_currentStepIndex > 0)
        {
            _currentStepIndex--;
            DisplayCurrentStep();
        }
    }

    [RelayCommand]
    private void NextStep()
    {
        if (_currentStepIndex < _allSteps.Count - 1)
        {
            _currentStepIndex++;
            DisplayCurrentStep();
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }
}

public class MediaItemDisplay
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public string TypeIcon { get; set; } = string.Empty;
}
