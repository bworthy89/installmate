using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Services;
using InstallVibe.Views;

namespace InstallVibe.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _welcomeMessage = "Welcome to InstallVibe!";

    public HomeViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    private void Logout()
    {
        _navigationService.NavigateTo<LoginView>();
    }
}
