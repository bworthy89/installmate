# InstallVibe

A modern WinUI 3 desktop application built with .NET 7, MVVM pattern, and SQLite.

## Project Structure

```
InstallVibe/
├── Models/              # Data models
├── Views/               # XAML views (UI)
│   ├── LoginView.xaml
│   └── HomeView.xaml
├── ViewModels/          # View models (presentation logic)
│   ├── LoginViewModel.cs
│   └── HomeViewModel.cs
├── Services/            # Business services
│   ├── INavigationService.cs
│   └── NavigationService.cs
├── Converters/          # XAML value converters
│   └── EmptyStringToVisibilityConverter.cs
├── App.xaml             # Application resources
├── App.xaml.cs          # Application startup and DI configuration
├── MainWindow.xaml      # Main application window shell
└── Program.cs           # Entry point
```

## Prerequisites

- Windows 10 version 1809 (build 17763) or later
- .NET 7.0 SDK
- Visual Studio 2022 (recommended) or Visual Studio Code with C# Dev Kit
- Windows App SDK 1.3 or later

## Build Instructions

### Using Command Line

1. **Restore NuGet packages:**
   ```bash
   dotnet restore InstallVibe.sln
   ```

2. **Build the solution:**
   ```bash
   dotnet build InstallVibe.sln -c Debug
   ```

3. **Run the application:**
   ```bash
   dotnet run --project InstallVibe/InstallVibe.csproj
   ```

### Using Visual Studio 2022

1. Open `InstallVibe.sln` in Visual Studio 2022
2. Set build configuration to Debug and platform to x64
3. Press F5 to build and run
4. Or use Build → Build Solution, then Debug → Start Debugging

## Features (Step 1 - Scaffold)

- ✅ WinUI 3 project structure
- ✅ MVVM architecture with CommunityToolkit.Mvvm
- ✅ Dependency injection using Microsoft.Extensions.DependencyInjection
- ✅ Navigation service for view switching
- ✅ LoginView with basic validation
- ✅ HomeView placeholder dashboard
- ✅ SQLite package ready for database integration

## Acceptance Criteria - Step 1

- [x] Full code compiles without errors
- [x] Application launches and displays main window
- [x] LoginView is displayed on startup
- [x] Navigation service can switch between LoginView and HomeView
- [x] Login button navigates to HomeView after entering credentials
- [x] Logout button returns to LoginView
- [x] Error message displays when credentials are empty

## Current Functionality

1. **Login View:**
   - Username and password input fields
   - Basic validation (non-empty fields required)
   - Login button to navigate to Home
   - Placeholder Register button

2. **Home View:**
   - Welcome message
   - Logout button to return to Login
   - Top navigation bar

## Next Steps

Future steps will add:
- User registration and authentication
- SQLite database integration
- Application installation tracking
- Installation management features
- Settings and preferences
- Accessibility enhancements
- Unit tests

## Technology Stack

- **UI Framework:** WinUI 3
- **Runtime:** .NET 7
- **MVVM:** CommunityToolkit.Mvvm
- **DI Container:** Microsoft.Extensions.DependencyInjection
- **Database:** Microsoft.Data.Sqlite (ready for integration)
- **Platform:** Windows App SDK 1.3

## Notes

- This is Step 1: Project scaffold with basic navigation
- Authentication is placeholder only (accepts any non-empty credentials)
- Database integration will be added in future steps
- The application uses modern Windows design patterns
