using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Models;
using InstallVibe.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InstallVibe.ViewModels;

public partial class StepEditorViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IMediaStorageService _mediaStorageService;
    private int _stepId;
    private Step? _step;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _instruction = string.Empty;

    [ObservableProperty]
    private string _requiredTools = string.Empty;

    [ObservableProperty]
    private string _safetyNotes = string.Empty;

    [ObservableProperty]
    private int _stepNumber = 1;

    [ObservableProperty]
    private ObservableCollection<MediaItemViewModel> _mediaItems = new();

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasUnsavedChanges = false;

    public bool HasNoMedia => MediaItems.Count == 0;

    public StepEditorViewModel(INavigationService navigationService, IMediaStorageService mediaStorageService)
    {
        _navigationService = navigationService;
        _mediaStorageService = mediaStorageService;
    }

    public void Initialize(int stepId)
    {
        _stepId = stepId;
        _ = LoadStepAsync();
    }

    private async Task LoadStepAsync()
    {
        IsLoading = true;

        try
        {
            using var scope = App.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IGuideRepository>();

            _step = await repository.GetStep(_stepId);
            if (_step != null)
            {
                StepNumber = _step.StepNumber;
                Title = _step.Title;
                Instruction = _step.Instruction;
                RequiredTools = _step.RequiredTools ?? string.Empty;
                SafetyNotes = _step.SafetyNotes ?? string.Empty;

                var mediaVms = _step.Media.Select(m => new MediaItemViewModel
                {
                    Id = m.Id,
                    FilePath = m.FilePath,
                    MediaType = m.MediaType,
                    TypeIcon = m.MediaType == MediaType.Image ? "\uE91B" : "\uE714" // Image or Video icon
                }).ToList();

                MediaItems = new ObservableCollection<MediaItemViewModel>(mediaVms);
            }

            HasUnsavedChanges = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading step: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddImage()
    {
        var filePath = await _mediaStorageService.PickAndSaveImageAsync();
        if (!string.IsNullOrEmpty(filePath))
        {
            var mediaItem = new MediaItemViewModel
            {
                Id = 0, // New item
                FilePath = filePath,
                MediaType = MediaType.Image,
                TypeIcon = "\uE91B"
            };

            MediaItems.Add(mediaItem);
            HasUnsavedChanges = true;
        }
    }

    [RelayCommand]
    private async Task AddVideo()
    {
        var filePath = await _mediaStorageService.PickAndSaveVideoAsync();
        if (!string.IsNullOrEmpty(filePath))
        {
            var mediaItem = new MediaItemViewModel
            {
                Id = 0, // New item
                FilePath = filePath,
                MediaType = MediaType.Video,
                TypeIcon = "\uE714"
            };

            MediaItems.Add(mediaItem);
            HasUnsavedChanges = true;
        }
    }

    [RelayCommand]
    private async Task DeleteMedia(MediaItemViewModel media)
    {
        if (media == null) return;

        MediaItems.Remove(media);

        // Delete from database if saved
        if (media.Id > 0)
        {
            try
            {
                using var scope = App.Services.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IGuideRepository>();
                await repository.DeleteMedia(media.Id);

                // Delete physical file
                await _mediaStorageService.DeleteMediaAsync(media.FilePath);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting media: {ex.Message}";
            }
        }

        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private async Task SaveStep()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Title is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(Instruction))
        {
            ErrorMessage = "Instruction is required";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            using var scope = App.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IGuideRepository>();

            if (_step != null)
            {
                // Update step
                _step.Title = Title;
                _step.Instruction = Instruction;
                _step.RequiredTools = RequiredTools;
                _step.SafetyNotes = SafetyNotes;

                await repository.UpdateStep(_step);

                // Add new media items
                foreach (var mediaVm in MediaItems.Where(m => m.Id == 0))
                {
                    var newMedia = new MediaItem
                    {
                        StepId = _stepId,
                        MediaType = mediaVm.MediaType,
                        FilePath = mediaVm.FilePath
                    };

                    var mediaId = await repository.AddMediaToStep(_stepId, newMedia);
                    mediaVm.Id = mediaId; // Update ID
                }

                ErrorMessage = "Step saved successfully!";
                HasUnsavedChanges = false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving step: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }

    partial void OnTitleChanged(string value) => HasUnsavedChanges = true;
    partial void OnInstructionChanged(string value) => HasUnsavedChanges = true;
    partial void OnRequiredToolsChanged(string value) => HasUnsavedChanges = true;
    partial void OnSafetyNotesChanged(string value) => HasUnsavedChanges = true;
}

public partial class MediaItemViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private MediaType _mediaType;

    [ObservableProperty]
    private string _typeIcon = string.Empty;

    public string FileName => System.IO.Path.GetFileName(FilePath);
}
