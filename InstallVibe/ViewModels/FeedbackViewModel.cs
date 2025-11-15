using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Models;
using InstallVibe.Services;

namespace InstallVibe.ViewModels;

public partial class FeedbackViewModel : ObservableObject
{
    private readonly IFeedbackService _feedbackService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private FeedbackType selectedFeedbackType = FeedbackType.General;

    [ObservableProperty]
    private FeedbackCategory selectedCategory = FeedbackCategory.Other;

    [ObservableProperty]
    private IssueSeverity selectedSeverity = IssueSeverity.Medium;

    [ObservableProperty]
    private int? rating;

    [ObservableProperty]
    private string subject = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string? stepsToReproduce;

    [ObservableProperty]
    private string? expectedBehavior;

    [ObservableProperty]
    private string? actualBehavior;

    [ObservableProperty]
    private bool isSubmitting;

    [ObservableProperty]
    private string? submitMessage;

    [ObservableProperty]
    private bool showBugFields;

    public ObservableCollection<FeedbackType> FeedbackTypes { get; }
    public ObservableCollection<FeedbackCategory> Categories { get; }
    public ObservableCollection<IssueSeverity> Severities { get; }

    public FeedbackViewModel(IFeedbackService feedbackService, IAuthService authService)
    {
        _feedbackService = feedbackService;
        _authService = authService;

        FeedbackTypes = new ObservableCollection<FeedbackType>(Enum.GetValues<FeedbackType>());
        Categories = new ObservableCollection<FeedbackCategory>(Enum.GetValues<FeedbackCategory>());
        Severities = new ObservableCollection<IssueSeverity>(Enum.GetValues<IssueSeverity>());
    }

    partial void OnSelectedFeedbackTypeChanged(FeedbackType value)
    {
        ShowBugFields = value == FeedbackType.BugReport;
    }

    [RelayCommand]
    private async Task SubmitFeedbackAsync()
    {
        if (string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(Description))
        {
            SubmitMessage = "Please fill in all required fields";
            return;
        }

        IsSubmitting = true;
        SubmitMessage = null;

        try
        {
            var currentUser = _authService.CurrentUser;
            var feedback = new FeedbackSubmission
            {
                UserId = currentUser?.Id ?? "unknown",
                UserEmail = currentUser?.Email ?? string.Empty,
                FeedbackType = SelectedFeedbackType,
                Category = SelectedCategory,
                Subject = Subject,
                Description = Description,
                Rating = Rating,
                Severity = SelectedSeverity,
                StepsToReproduce = StepsToReproduce,
                ExpectedBehavior = ExpectedBehavior,
                ActualBehavior = ActualBehavior,
                AppVersion = GetAppVersion()
            };

            var success = await _feedbackService.SubmitFeedbackAsync(feedback);

            if (success)
            {
                SubmitMessage = "Thank you! Your feedback has been submitted.";
                ClearForm();
            }
            else
            {
                SubmitMessage = "Failed to submit feedback. Please try again.";
            }
        }
        catch (Exception ex)
        {
            SubmitMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        Subject = string.Empty;
        Description = string.Empty;
        StepsToReproduce = null;
        ExpectedBehavior = null;
        ActualBehavior = null;
        Rating = null;
        SelectedFeedbackType = FeedbackType.General;
        SelectedCategory = FeedbackCategory.Other;
        SelectedSeverity = IssueSeverity.Medium;
        SubmitMessage = null;
    }

    private string GetAppVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "1.0.0.0";
    }
}
