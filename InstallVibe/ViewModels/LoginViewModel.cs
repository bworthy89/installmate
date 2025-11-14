using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Models;
using InstallVibe.Services;
using InstallVibe.Views;

namespace InstallVibe.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoggingIn = false;

    public LoginViewModel(INavigationService navigationService, IAuthService authService)
    {
        _navigationService = navigationService;
        _authService = authService;
    }

    [RelayCommand]
    private async Task Login()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required.";
            return;
        }

        IsLoggingIn = true;
        ErrorMessage = string.Empty;

        try
        {
            // Attempt login
            var user = await _authService.Login(Username, Password);

            if (user != null)
            {
                // Login successful - navigate based on role
                ErrorMessage = string.Empty;

                // Both Admin and Technician go to HomeView for now
                // You can create separate views later (e.g., AdminHomeView)
                _navigationService.NavigateTo<HomeView>();
            }
            else
            {
                // Login failed
                ErrorMessage = "Invalid username or password.";
            }
        }
        catch (System.Exception ex)
        {
            ErrorMessage = $"Login error: {ex.Message}";
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    [RelayCommand]
    private void Register()
    {
        // Placeholder for registration
        ErrorMessage = "Registration not yet implemented.";
    }
}
