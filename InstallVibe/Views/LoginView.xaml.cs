using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels;

namespace InstallVibe.Views;

public sealed partial class LoginView : Page
{
    public LoginViewModel ViewModel { get; }

    public LoginView(LoginViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }
}
