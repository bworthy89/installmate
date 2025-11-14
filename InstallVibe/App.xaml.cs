using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.EntityFrameworkCore;
using InstallVibe.Data;
using InstallVibe.Services;
using InstallVibe.ViewModels;
using InstallVibe.Views;
using System;
using System.IO;
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
        // Initialize databases and seed data
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

        // Get database path
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataFolder, "InstallVibe");
        Directory.CreateDirectory(appFolder);
        var dbPath = Path.Combine(appFolder, "installvibe.db");

        // EF Core DbContext
        services.AddDbContext<InstallVibeDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Legacy Database Service (for auth tables)
        services.AddSingleton<IDatabaseService, DatabaseService>();

        // Authentication Services
        services.AddSingleton<IAuthService, AuthService>();

        // Repository Services
        services.AddScoped<IGuideRepository, GuideRepository>();

        // Navigation Service
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<GuideLibraryViewModel>();
        services.AddTransient<GuideDetailViewModel>();
        services.AddTransient<StepViewerViewModel>();

        // Views
        services.AddTransient<LoginView>();
        services.AddTransient<HomeView>();
        services.AddTransient<GuideLibraryView>();
        services.AddTransient<GuideDetailView>();
        services.AddTransient<StepViewerView>();

        // Main Window
        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }

    private static async Task InitializeDatabaseAsync()
    {
        // Initialize legacy auth database
        var databaseService = Services.GetRequiredService<IDatabaseService>();
        var authService = Services.GetRequiredService<IAuthService>();

        await databaseService.InitializeAsync();
        await authService.SeedAdmin();

        // Initialize EF Core database and apply migrations
        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<InstallVibeDbContext>();
            await context.Database.EnsureCreatedAsync();
        }
    }
}
