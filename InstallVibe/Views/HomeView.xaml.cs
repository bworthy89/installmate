using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels;

namespace InstallVibe.Views;

public sealed partial class HomeView : Page
{
    public HomeViewModel ViewModel { get; }

    public HomeView()
    {
        ViewModel = App.Services.GetRequiredService<HomeViewModel>();
        InitializeComponent();
    }
}
