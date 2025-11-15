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

public partial class GuideEditorViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private int? _guideId;
    private Guide? _guide;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private int? _estimatedDurationMinutes;

    [ObservableProperty]
    private ObservableCollection<StepEditorItem> _steps = new();

    [ObservableProperty]
    private StepEditorItem? _selectedStep;

    [ObservableProperty]
    private ObservableCollection<string> _categories = new();

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _hasUnsavedChanges = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _pageTitle = "Create New Guide";

    public bool IsExistingGuide => _guideId.HasValue;
    public bool HasNoSteps => Steps.Count == 0;
    public Microsoft.UI.Xaml.Controls.InfoBarSeverity MessageSeverity =>
        string.IsNullOrEmpty(ErrorMessage) ? Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational :
        ErrorMessage.Contains("success") ? Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success :
        Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning;
    public string MessageTitle => ErrorMessage.Contains("success") ? "Success" : "Message";

    public GuideEditorViewModel(INavigationService navigationService, IAuthService authService)
    {
        _navigationService = navigationService;
        _authService = authService;

        // Populate common categories
        Categories = new ObservableCollection<string>
        {
            "HVAC",
            "Electrical",
            "Plumbing",
            "Carpentry",
            "Automotive",
            "Appliances",
            "Security",
            "Networking",
            "General"
        };
    }

    public async Task InitializeAsync(Dictionary<string, object>? parameters)
    {
        if (parameters != null && parameters.TryGetValue("GuideId", out var guideIdObj) && guideIdObj is int guideId)
        {
            _guideId = guideId;
            PageTitle = "Edit Guide";
            await LoadGuideAsync(guideId);
        }
        else
        {
            // New guide
            _guideId = null;
            PageTitle = "Create New Guide";
        }
    }

    private async Task LoadGuideAsync(int guideId)
    {
        IsLoading = true;

        try
        {
            using var scope = App.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IGuideRepository>();

            _guide = await repository.GetGuide(guideId);
            if (_guide != null)
            {
                Title = _guide.Title;
                Description = _guide.Description;
                Category = _guide.Category;
                EstimatedDurationMinutes = _guide.EstimatedDurationMinutes;

                var stepItems = _guide.Steps.OrderBy(s => s.StepNumber).Select((s, index) => new StepEditorItem
                {
                    Id = s.Id,
                    StepNumber = s.StepNumber,
                    Title = s.Title,
                    Instruction = s.Instruction,
                    RequiredTools = s.RequiredTools ?? string.Empty,
                    SafetyNotes = s.SafetyNotes ?? string.Empty,
                    MediaCount = s.Media.Count
                }).ToList();

                Steps = new ObservableCollection<StepEditorItem>(stepItems);
            }

            HasUnsavedChanges = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading guide: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddStep()
    {
        var newStepNumber = Steps.Count + 1;
        var newStep = new StepEditorItem
        {
            Id = 0, // 0 indicates new step
            StepNumber = newStepNumber,
            Title = $"Step {newStepNumber}",
            Instruction = "",
            RequiredTools = "",
            SafetyNotes = "",
            MediaCount = 0
        };

        Steps.Add(newStep);
        SelectedStep = newStep;
        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private async Task DeleteStep(StepEditorItem step)
    {
        if (step == null) return;

        Steps.Remove(step);

        // Renumber steps
        for (int i = 0; i < Steps.Count; i++)
        {
            Steps[i].StepNumber = i + 1;
        }

        // If step was already saved to database, delete it
        if (step.Id > 0)
        {
            try
            {
                using var scope = App.Services.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IGuideRepository>();
                await repository.DeleteStep(step.Id);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting step: {ex.Message}";
            }
        }

        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void MoveStepUp(StepEditorItem step)
    {
        var index = Steps.IndexOf(step);
        if (index > 0)
        {
            Steps.Move(index, index - 1);
            RenumberSteps();
            HasUnsavedChanges = true;
        }
    }

    [RelayCommand]
    private void MoveStepDown(StepEditorItem step)
    {
        var index = Steps.IndexOf(step);
        if (index < Steps.Count - 1)
        {
            Steps.Move(index, index + 1);
            RenumberSteps();
            HasUnsavedChanges = true;
        }
    }

    private void RenumberSteps()
    {
        for (int i = 0; i < Steps.Count; i++)
        {
            Steps[i].StepNumber = i + 1;
        }
    }

    [RelayCommand]
    private void EditStep(StepEditorItem step)
    {
        // Navigate to step editor view
        if (step.Id > 0)
        {
            _navigationService.NavigateToStepEditor(step.Id);
        }
    }

    [RelayCommand]
    private async Task SaveGuide()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Title is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            ErrorMessage = "Description is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(Category))
        {
            ErrorMessage = "Category is required";
            return;
        }

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
                return;
            }

            if (_guideId.HasValue && _guide != null)
            {
                // Update existing guide
                _guide.Title = Title;
                _guide.Description = Description;
                _guide.Category = Category;
                _guide.EstimatedDurationMinutes = EstimatedDurationMinutes;

                await repository.UpdateGuide(_guide);

                // Update steps
                foreach (var stepItem in Steps)
                {
                    if (stepItem.Id > 0)
                    {
                        // Update existing step
                        var step = await repository.GetStep(stepItem.Id);
                        if (step != null)
                        {
                            step.StepNumber = stepItem.StepNumber;
                            step.Title = stepItem.Title;
                            step.Instruction = stepItem.Instruction;
                            step.RequiredTools = stepItem.RequiredTools;
                            step.SafetyNotes = stepItem.SafetyNotes;
                            await repository.UpdateStep(step);
                        }
                    }
                    else
                    {
                        // Create new step
                        var newStep = new Step
                        {
                            GuideId = _guide.Id,
                            StepNumber = stepItem.StepNumber,
                            Title = stepItem.Title,
                            Instruction = stepItem.Instruction,
                            RequiredTools = stepItem.RequiredTools,
                            SafetyNotes = stepItem.SafetyNotes
                        };
                        await repository.CreateStep(_guide.Id, newStep);
                    }
                }

                ErrorMessage = "Guide updated successfully!";
            }
            else
            {
                // Create new guide
                var newGuide = new Guide
                {
                    Title = Title,
                    Description = Description,
                    Category = Category,
                    CreatedByUserId = currentUser.Id,
                    EstimatedDurationMinutes = EstimatedDurationMinutes,
                    Steps = Steps.Select(s => new Step
                    {
                        StepNumber = s.StepNumber,
                        Title = s.Title,
                        Instruction = s.Instruction,
                        RequiredTools = s.RequiredTools,
                        SafetyNotes = s.SafetyNotes
                    }).ToList()
                };

                var guideId = await repository.CreateGuide(newGuide);
                _guideId = guideId;
                _guide = newGuide;
                PageTitle = "Edit Guide";

                ErrorMessage = "Guide created successfully!";
            }

            HasUnsavedChanges = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving guide: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteGuide()
    {
        if (!_guideId.HasValue) return;

        // Confirmation should be done in view
        try
        {
            using var scope = App.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IGuideRepository>();

            await repository.DeleteGuide(_guideId.Value);

            // Navigate back
            _navigationService.GoBack();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting guide: {ex.Message}";
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }

    partial void OnTitleChanged(string value) => HasUnsavedChanges = true;
    partial void OnDescriptionChanged(string value) => HasUnsavedChanges = true;
    partial void OnCategoryChanged(string value) => HasUnsavedChanges = true;
    partial void OnEstimatedDurationMinutesChanged(int? value) => HasUnsavedChanges = true;
}

public partial class StepEditorItem : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private int _stepNumber;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _instruction = string.Empty;

    [ObservableProperty]
    private string _requiredTools = string.Empty;

    [ObservableProperty]
    private string _safetyNotes = string.Empty;

    [ObservableProperty]
    private int _mediaCount;

    public string StepNumberText => $"Step {StepNumber}";
    public string MediaText => MediaCount > 0 ? $"{MediaCount} media file(s)" : "No media";
}
