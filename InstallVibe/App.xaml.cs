using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using InstallVibe.Services;
using InstallVibe.ViewModels;
using InstallVibe.Views;
using System;

namespace InstallVibe;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static Window MainWindow { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
        Services = ConfigureServices();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = Services.GetRequiredService<MainWindow>();
        MainWindow.Activate();

        // Navigate to login view on startup
        var navigationService = Services.GetRequiredService<INavigationService>();
        navigationService.NavigateTo<LoginView>();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<HomeViewModel>();

        // Views
        services.AddTransient<LoginView>();
        services.AddTransient<HomeView>();

        // Main Window
        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }
}
