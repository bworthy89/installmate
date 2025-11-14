using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Services;
using InstallVibe.Views;

namespace InstallVibe.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public LoginViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    private void Login()
    {
        // Simple validation for now
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required.";
            return;
        }

        // For now, accept any non-empty credentials
        ErrorMessage = string.Empty;
        _navigationService.NavigateTo<HomeView>();
    }

    [RelayCommand]
    private void Register()
    {
        // Placeholder for registration
        ErrorMessage = "Registration not yet implemented.";
    }
}
