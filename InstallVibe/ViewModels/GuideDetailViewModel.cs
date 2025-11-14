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

public partial class GuideDetailViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private int _guideId;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private int _totalSteps = 0;

    [ObservableProperty]
    private int _estimatedDurationMinutes = 0;

    [ObservableProperty]
    private string _createdBy = string.Empty;

    [ObservableProperty]
    private ObservableCollection<StepListItem> _steps = new();

    [ObservableProperty]
    private int _completedSteps = 0;

    [ObservableProperty]
    private double _progressPercentage = 0;

    [ObservableProperty]
    private string _startButtonText = "Start Guide";

    [ObservableProperty]
    private bool _isLoading = false;

    public GuideDetailViewModel(INavigationService navigationService, IAuthService authService)
    {
        _navigationService = navigationService;
        _authService = authService;
    }

    public async Task InitializeAsync(int guideId)
    {
        _guideId = guideId;
        await LoadGuideAsync();
    }

    public void Initialize(Dictionary<string, object> parameters)
    {
        if (parameters.TryGetValue("GuideId", out var guideIdObj) && guideIdObj is int guideId)
        {
            _ = InitializeAsync(guideId);
        }
    }

    private async Task LoadGuideAsync()
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

            // Load guide
            var guide = await repository.GetGuide(_guideId);
            if (guide == null)
            {
                IsLoading = false;
                return;
            }

            // Populate properties
            Title = guide.Title;
            Description = guide.Description;
            Category = guide.Category;
            TotalSteps = guide.Steps.Count;
            EstimatedDurationMinutes = guide.EstimatedDurationMinutes ?? 0;
            CreatedBy = guide.CreatedByUser?.Username ?? "Unknown";

            // Load progress
            var progress = await repository.GetProgress(_guideId, currentUser.Id);
            var completedStepIds = progress?.CompletedStepIds ?? new List<int>();
            CompletedSteps = completedStepIds.Count;
            ProgressPercentage = TotalSteps > 0 ? (CompletedSteps / (double)TotalSteps) * 100 : 0;

            // Update button text
            if (CompletedSteps == 0)
            {
                StartButtonText = "Start Guide";
            }
            else if (CompletedSteps < TotalSteps)
            {
                StartButtonText = "Continue Guide";
            }
            else
            {
                StartButtonText = "Review Guide";
            }

            // Populate steps
            var stepItems = guide.Steps.OrderBy(s => s.StepNumber).Select(s => new StepListItem
            {
                Id = s.Id,
                StepNumber = s.StepNumber,
                Title = s.Title,
                IsCompleted = completedStepIds.Contains(s.Id)
            }).ToList();

            Steps = new ObservableCollection<StepListItem>(stepItems);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading guide: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void StartContinueGuide()
    {
        // Find the first incomplete step or start from the beginning
        var firstIncompleteStepIndex = Steps
            .Select((step, index) => new { step, index })
            .FirstOrDefault(x => !x.step.IsCompleted)?.index ?? 0;

        _navigationService.NavigateToStepViewer(_guideId, firstIncompleteStepIndex);
    }

    [RelayCommand]
    private void NavigateToStep(StepListItem step)
    {
        var stepIndex = Steps.IndexOf(step);
        _navigationService.NavigateToStepViewer(_guideId, stepIndex);
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }
}

public class StepListItem
{
    public int Id { get; set; }
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public string CompletionIcon => IsCompleted ? "✓" : "";
}
