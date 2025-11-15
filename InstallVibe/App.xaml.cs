using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.EntityFrameworkCore;
using InstallVibe.Data;
using InstallVibe.Services;
using InstallVibe.ViewModels;
using InstallVibe.Views;
using System;
using System.Collections.Generic;
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

        // Single database for everything using EF Core
        var dbPath = Path.Combine(appFolder, "installvibe.db");

        // EF Core DbContext
        services.AddDbContext<InstallVibeDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Authentication Services
        services.AddSingleton<IAuthService, AuthService>();

        // Repository Services
        services.AddScoped<IGuideRepository, GuideRepository>();

        // Media Storage Service
        services.AddSingleton<IMediaStorageService, MediaStorageService>();

        // Settings Service
        services.AddSingleton<ISettingsService, SettingsService>();

        // Navigation Service
        services.AddSingleton<INavigationService, NavigationService>();

        // Update Services (Step 9)
        services.AddSingleton<INetworkService, NetworkService>();
        services.AddSingleton<IUpdateService, UpdateService>();

        // Feedback Service (Step 10)
        services.AddSingleton<IFeedbackService, FeedbackService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<GuideLibraryViewModel>();
        services.AddTransient<GuideDetailViewModel>();
        services.AddTransient<StepViewerViewModel>();
        services.AddTransient<GuideViewerViewModel>();

        // Admin ViewModels
        services.AddTransient<AdminDashboardViewModel>();
        services.AddTransient<GuideEditorViewModel>();
        services.AddTransient<StepEditorViewModel>();

        // Settings ViewModel
        services.AddTransient<SettingsViewModel>();

        // Update ViewModel (Step 9)
        services.AddTransient<UpdateViewModel>();

        // Step 10 ViewModels
        services.AddTransient<FeedbackViewModel>();
        services.AddTransient<DiagnosticsViewModel>();

        // Views
        services.AddTransient<LoginView>();
        services.AddTransient<HomeView>();
        services.AddTransient<GuideLibraryView>();
        services.AddTransient<GuideDetailView>();
        services.AddTransient<StepViewerView>();
        services.AddTransient<GuideViewerView>();

        // Admin Views
        services.AddTransient<Views.Admin.AdminDashboardView>();
        services.AddTransient<Views.Admin.GuideEditorView>();
        services.AddTransient<Views.Admin.StepEditorView>();

        // Settings View
        services.AddTransient<SettingsView>();

        // Step 10 Views
        services.AddTransient<DiagnosticsView>();

        // Main Window
        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }

    private static async Task InitializeDatabaseAsync()
    {
        // Initialize EF Core database and ensure seed data exists
        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<InstallVibeDbContext>();

            // Delete and recreate database to ensure fresh seed data (development only)
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }

        // Seed admin user first
        var authService = Services.GetRequiredService<IAuthService>();
        await authService.SeedAdmin();

        // Seed sample guide data
        await SeedGuideDataAsync();
    }

    private static async Task SeedGuideDataAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InstallVibeDbContext>();

        // Check if guide already exists
        if (await context.Guides.AnyAsync())
        {
            return; // Guides already seeded
        }

        // Create sample HVAC guide
        var hvacGuide = new Models.Guide
        {
            Title = "Standard HVAC Unit Installation",
            Description = "Complete installation guide for residential HVAC units. Includes safety procedures, mounting instructions, and electrical connections.",
            Category = "HVAC",
            CreatedByUserId = 1, // Admin user
            EstimatedDurationMinutes = 180,
            Steps = new List<Models.Step>
            {
                new Models.Step
                {
                    StepNumber = 1,
                    Title = "Pre-installation Safety Check",
                    Instruction = "Before beginning installation, ensure all power to the installation area is shut off at the circuit breaker. Verify the power is off using a voltage tester. Wear appropriate PPE including safety glasses and work gloves.",
                    RequiredTools = "Voltage tester, Safety glasses, Work gloves",
                    SafetyNotes = "DANGER: Always verify power is off before working with electrical equipment. Lock out and tag the breaker box to prevent accidental power restoration.",
                    Media = new List<Models.MediaItem>
                    {
                        new Models.MediaItem
                        {
                            MediaType = Models.MediaType.Image,
                            FilePath = "/media/hvac/safety-check.jpg"
                        }
                    }
                },
                new Models.Step
                {
                    StepNumber = 2,
                    Title = "Mounting the Unit",
                    Instruction = "Position the HVAC unit on the mounting bracket, ensuring it is level. Use a carpenter's level to verify both horizontal and vertical alignment. Secure the unit using the provided mounting bolts, tightening in a cross pattern to ensure even pressure.",
                    RequiredTools = "Carpenter's level, Socket wrench set, Mounting bolts (included)",
                    SafetyNotes = "Unit weighs 75+ lbs. Use proper lifting technique or get assistance. Ensure mounting bracket is rated for unit weight."
                },
                new Models.Step
                {
                    StepNumber = 3,
                    Title = "Electrical Connection",
                    Instruction = "Connect the electrical wiring according to the wiring diagram provided with the unit. Match wire colors: black to black (hot), white to white (neutral), and green/bare to ground. Use wire nuts to secure all connections. Install the electrical cover plate.",
                    RequiredTools = "Wire strippers, Screwdriver set, Wire nuts, Electrical tape",
                    SafetyNotes = "DANGER: Ensure power remains off during all electrical work. Double-check all connections before restoring power. If unsure, consult a licensed electrician."
                }
            }
        };

        context.Guides.Add(hvacGuide);
        await context.SaveChangesAsync();
    }
}
