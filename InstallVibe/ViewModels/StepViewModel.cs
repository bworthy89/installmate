using CommunityToolkit.Mvvm.ComponentModel;
using InstallVibe.Models;

namespace InstallVibe.ViewModels;

public partial class StepViewModel : ObservableObject
{
    private readonly Step _step;

    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private bool _isSelected;

    public int Id => _step.Id;
    public int StepNumber => _step.StepNumber;
    public string Title => _step.Title;
    public string Instruction => _step.Instruction;
    public string? RequiredTools => _step.RequiredTools;
    public string? SafetyNotes => _step.SafetyNotes;
    public int Index { get; set; }

    public Step Step => _step;

    public string CompletionIcon => IsCompleted ? "\uE73E" : "\uE739"; // CheckMark : Circle
    public string StepNumberText => $"Step {StepNumber}";

    public StepViewModel(Step step, int index, bool isCompleted)
    {
        _step = step;
        Index = index;
        _isCompleted = isCompleted;
    }

    partial void OnIsCompletedChanged(bool value)
    {
        OnPropertyChanged(nameof(CompletionIcon));
    }
}
