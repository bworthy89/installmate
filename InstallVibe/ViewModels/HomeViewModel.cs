using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Services;
using InstallVibe.Views;

namespace InstallVibe.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _welcomeMessage = "Welcome to InstallVibe!";

    public HomeViewModel(INavigationService navigationService, IAuthService authService)
    {
        _navigationService = navigationService;
        _authService = authService;

        // Update welcome message with current user info
        var currentUser = _authService.GetCurrentUser();
        if (currentUser != null)
        {
            WelcomeMessage = $"Welcome, {currentUser.Username}! ({currentUser.Role})";
        }
    }

    [RelayCommand]
    private void ViewGuides()
    {
        _navigationService.NavigateToGuideLibrary();
    }

    [RelayCommand]
    private async Task Logout()
    {
        await _authService.Logout();
        _navigationService.NavigateTo<LoginView>();
    }
}
