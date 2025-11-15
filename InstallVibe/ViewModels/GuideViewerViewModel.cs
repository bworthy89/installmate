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
using Microsoft.UI.Xaml.Controls;

namespace InstallVibe.ViewModels;

public partial class GuideViewerViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private int _guideId;
    private Guide? _guide;
    private List<int> _completedStepIds = new();

    [ObservableProperty]
    private string _guideTitle = string.Empty;

    [ObservableProperty]
    private string _guideDescription = string.Empty;

    [ObservableProperty]
    private ObservableCollection<StepViewModel> _steps = new();

    [ObservableProperty]
    private StepViewModel? _currentStep;

    [ObservableProperty]
    private double _progressPercent = 0;

    [ObservableProperty]
    private string _progressText = "0 / 0 steps";

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isAllCompleted = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool CanGoPrevious => CurrentStep != null && CurrentStep.Index > 0;
    public bool CanGoNext => CurrentStep != null && CurrentStep.Index < Steps.Count - 1;

    public GuideViewerViewModel(INavigationService navigationService, IAuthService authService)
    {
        _navigationService = navigationService;
        _authService = authService;
    }

    public void Initialize(Dictionary<string, object> parameters)
    {
        if (parameters.TryGetValue("GuideId", out var guideIdObj) && guideIdObj is int guideId)
        {
            _guideId = guideId;
            _ = LoadGuideAsync();
        }
    }

    public async Task LoadGuideAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            using var scope = App.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IGuideRepository>();
            var currentUser = _authService.GetCurrentUser();

            if (currentUser == null)
            {
                ErrorMessage = "User not authenticated";
                IsLoading = false;
                return;
            }

            // Load guide with steps
            _guide = await repository.GetGuide(_guideId);
            if (_guide == null)
            {
                ErrorMessage = "Guide not found";
                IsLoading = false;
                return;
            }

            GuideTitle = _guide.Title;
            GuideDescription = _guide.Description;

            // Load progress
            var progress = await repository.GetProgress(_guideId, currentUser.Id);
            _completedStepIds = progress?.CompletedStepIds ?? new List<int>();

            // Build step view models
            var stepViewModels = _guide.Steps
                .OrderBy(s => s.StepNumber)
                .Select((step, index) => new StepViewModel(
                    step,
                    index,
                    _completedStepIds.Contains(step.Id)))
                .ToList();

            Steps = new ObservableCollection<StepViewModel>(stepViewModels);

            // Set current step to first incomplete or first step
            CurrentStep = Steps.FirstOrDefault(s => !s.IsCompleted) ?? Steps.FirstOrDefault();

            UpdateProgress();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading guide: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in LoadGuideAsync: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task MarkStepComplete()
    {
        if (CurrentStep == null) return;

        // Optimistic UI update
        var wasCompleted = CurrentStep.IsCompleted;
        CurrentStep.IsCompleted = !CurrentStep.IsCompleted;

        if (CurrentStep.IsCompleted)
        {
            if (!_completedStepIds.Contains(CurrentStep.Id))
            {
                _completedStepIds.Add(CurrentStep.Id);
            }
        }
        else
        {
            _completedStepIds.Remove(CurrentStep.Id);
        }

        UpdateProgress();

        // Persist to database
        try
        {
            using var scope = App.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IGuideRepository>();
            var currentUser = _authService.GetCurrentUser();

            if (currentUser != null)
            {
                await repository.SaveProgress(_guideId, currentUser.Id, _completedStepIds);
            }
        }
        catch (Exception ex)
        {
            // Revert on error
            CurrentStep.IsCompleted = wasCompleted;
            if (wasCompleted)
            {
                _completedStepIds.Add(CurrentStep.Id);
            }
            else
            {
                _completedStepIds.Remove(CurrentStep.Id);
            }
            UpdateProgress();

            ErrorMessage = $"Failed to save progress: {ex.Message}. Please try again.";
            System.Diagnostics.Debug.WriteLine($"Error saving progress: {ex}");
        }
    }

    [RelayCommand]
    private void SelectStep(StepViewModel step)
    {
        if (CurrentStep != null)
        {
            CurrentStep.IsSelected = false;
        }

        CurrentStep = step;
        CurrentStep.IsSelected = true;

        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
    }

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void PreviousStep()
    {
        if (CurrentStep != null && CurrentStep.Index > 0)
        {
            SelectStep(Steps[CurrentStep.Index - 1]);
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void NextStep()
    {
        if (CurrentStep != null && CurrentStep.Index < Steps.Count - 1)
        {
            SelectStep(Steps[CurrentStep.Index + 1]);
        }
    }

    [RelayCommand]
    private async Task ResetProgress()
    {
        // This will be called after user confirms in the view
        try
        {
            // Clear all completion states
            foreach (var step in Steps)
            {
                step.IsCompleted = false;
            }

            _completedStepIds.Clear();
            UpdateProgress();

            // Persist to database
            using var scope = App.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IGuideRepository>();
            var currentUser = _authService.GetCurrentUser();

            if (currentUser != null)
            {
                await repository.SaveProgress(_guideId, currentUser.Id, _completedStepIds);
            }

            // Go to first step
            if (Steps.Count > 0)
            {
                SelectStep(Steps[0]);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to reset progress: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error resetting progress: {ex}");
        }
    }

    [RelayCommand]
    private void FinishGuide()
    {
        // Navigate back to guide detail or guide library
        _navigationService.GoBack();
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }

    private void UpdateProgress()
    {
        var completedCount = Steps.Count(s => s.IsCompleted);
        var totalCount = Steps.Count;

        ProgressPercent = totalCount > 0 ? (completedCount / (double)totalCount) * 100 : 0;
        ProgressText = $"{completedCount} / {totalCount} steps";
        IsAllCompleted = completedCount == totalCount && totalCount > 0;
    }

    partial void OnCurrentStepChanged(StepViewModel? value)
    {
        PreviousStepCommand.NotifyCanExecuteChanged();
        NextStepCommand.NotifyCanExecuteChanged();
    }
}
