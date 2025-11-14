using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using InstallVibe.Services;
using InstallVibe.ViewModels;
using InstallVibe.Views;
using System;
using System.Threading.Tasks;

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

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Initialize database and seed admin user
        await InitializeDatabaseAsync();

        MainWindow = Services.GetRequiredService<MainWindow>();
        MainWindow.Activate();

        // Navigate to login view on startup
        var navigationService = Services.GetRequiredService<INavigationService>();
        navigationService.NavigateTo<LoginView>();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Database and Authentication Services
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<IAuthService, AuthService>();

        // Navigation Service
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

    private static async Task InitializeDatabaseAsync()
    {
        var databaseService = Services.GetRequiredService<IDatabaseService>();
        var authService = Services.GetRequiredService<IAuthService>();

        // Initialize database schema
        await databaseService.InitializeAsync();

        // Seed admin user
        await authService.SeedAdmin();
    }
}
