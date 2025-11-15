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

public partial class AdminDashboardViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private ObservableCollection<GuideListItem> _recentGuides = new();

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    public AdminDashboardViewModel(INavigationService navigationService, IAuthService authService)
    {
        _navigationService = navigationService;
        _authService = authService;

        var currentUser = _authService.GetCurrentUser();
        WelcomeMessage = $"Welcome, {currentUser?.Username ?? "Admin"}!";
    }

    public async Task LoadRecentGuidesAsync()
    {
        IsLoading = true;

        try
        {
            using var scope = App.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IGuideRepository>();

            var guides = await repository.GetAllGuides();
            var guideList = guides.Take(5).Select(g => new GuideListItem
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                Category = g.Category,
                TotalSteps = g.Steps.Count
            }).ToList();

            RecentGuides = new ObservableCollection<GuideListItem>(guideList);
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

    [RelayCommand]
    private void CreateNewGuide()
    {
        _navigationService.NavigateToGuideEditor(null);
    }

    [RelayCommand]
    private void ManageGuides()
    {
        // Navigate to full guide list (could be GuideLibraryView with admin mode)
        _navigationService.NavigateToGuideLibrary();
    }

    [RelayCommand]
    private void EditGuide(int guideId)
    {
        _navigationService.NavigateToGuideEditor(guideId);
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }
}
