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

public partial class GuideLibraryViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private List<GuideListItem> _allGuides = new();

    [ObservableProperty]
    private ObservableCollection<GuideListItem> _guides = new();

    [ObservableProperty]
    private ObservableCollection<string> _categories = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "All";

    [ObservableProperty]
    private bool _isLoading = false;

    public GuideLibraryViewModel(INavigationService navigationService, IAuthService authService)
    {
        _navigationService = navigationService;
        _authService = authService;
    }

    public async Task LoadGuidesAsync()
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

            // Load all guides
            var allGuidesData = await repository.GetAllGuides();

            // Build guide list items with progress
            _allGuides = new List<GuideListItem>();

            foreach (var guide in allGuidesData)
            {
                var progress = await repository.GetProgress(guide.Id, currentUser.Id);
                var completedSteps = progress?.CompletedStepIds?.Count ?? 0;
                var totalSteps = guide.Steps.Count;
                var progressPercentage = totalSteps > 0 ? (completedSteps / (double)totalSteps) * 100 : 0;

                _allGuides.Add(new GuideListItem
                {
                    Id = guide.Id,
                    Title = guide.Title,
                    Description = guide.Description,
                    Category = guide.Category,
                    TotalSteps = totalSteps,
                    CompletedSteps = completedSteps,
                    ProgressPercentage = progressPercentage,
                    EstimatedDurationMinutes = guide.EstimatedDurationMinutes ?? 0
                });
            }

            // Extract unique categories
            var uniqueCategories = _allGuides.Select(g => g.Category).Distinct().OrderBy(c => c).ToList();
            Categories = new ObservableCollection<string>(new[] { "All" }.Concat(uniqueCategories));

            // Apply filters
            ApplyFilters();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading guides: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allGuides.AsEnumerable();

        // Apply category filter
        if (SelectedCategory != "All")
        {
            filtered = filtered.Where(g => g.Category == SelectedCategory);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(g =>
                g.Title.ToLower().Contains(searchLower) ||
                g.Description.ToLower().Contains(searchLower));
        }

        Guides = new ObservableCollection<GuideListItem>(filtered);
    }

    [RelayCommand]
    private void NavigateToGuide(GuideListItem guide)
    {
        System.Diagnostics.Debug.WriteLine($"NavigateToGuide called for Guide ID: {guide.Id}, Title: {guide.Title}");
        try
        {
            _navigationService.NavigateToGuideDetail(guide.Id);
            System.Diagnostics.Debug.WriteLine("Navigation completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }
}

public class GuideListItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
    public double ProgressPercentage { get; set; }
    public int EstimatedDurationMinutes { get; set; }

    public string ProgressText => $"{CompletedSteps} of {TotalSteps} steps";
    public string DurationText => EstimatedDurationMinutes > 0
        ? $"~{EstimatedDurationMinutes} min"
        : "";
}
