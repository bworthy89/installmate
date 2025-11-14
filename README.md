# InstallVibe

A modern WinUI 3 desktop application built with .NET 7, MVVM pattern, and SQLite.

## Project Structure

```
InstallVibe/
├── Data/                # EF Core database context
│   └── InstallVibeDbContext.cs  # EF Core DbContext
├── Models/              # Data models
│   ├── User.cs          # User model
│   ├── UserRole.cs      # User role enum
│   ├── Guide.cs         # Installation guide model
│   ├── Step.cs          # Guide step model
│   ├── MediaItem.cs     # Media (image/video) model
│   ├── MediaType.cs     # Media type enum
│   └── GuideProgress.cs # User progress tracking
├── Views/               # XAML views (UI)
│   ├── LoginView.xaml
│   └── HomeView.xaml
├── ViewModels/          # View models (presentation logic)
│   ├── LoginViewModel.cs
│   └── HomeViewModel.cs
├── Services/            # Business services
│   ├── INavigationService.cs
│   ├── NavigationService.cs
│   ├── IDatabaseService.cs      # Database initialization
│   ├── DatabaseService.cs
│   ├── IAuthService.cs          # Authentication interface
│   ├── AuthService.cs           # PBKDF2 authentication
│   ├── IGuideRepository.cs      # Guide repository interface
│   └── GuideRepository.cs       # EF Core guide repository
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

## Features

### Step 1 - Project Scaffold ✅
- ✅ WinUI 3 project structure
- ✅ MVVM architecture with CommunityToolkit.Mvvm
- ✅ Dependency injection using Microsoft.Extensions.DependencyInjection
- ✅ Navigation service for view switching
- ✅ LoginView and HomeView
- ✅ Unpackaged self-contained deployment

### Step 2 - Authentication & Role Model ✅
- ✅ User model with Id, Username, PasswordHash, Role
- ✅ UserRole enum (Admin, Technician)
- ✅ SQLite database integration
- ✅ PBKDF2 password hashing (100,000 iterations, SHA256)
- ✅ Authentication service with Login, Logout, GetCurrentUser, SeedAdmin
- ✅ Seeded admin account (username: `admin`, password: `admin123`)
- ✅ Role-based navigation
- ✅ Secure credential validation

### Step 3 - Database Models & Persistence Layer ✅
- ✅ Domain models (Guide, Step, MediaItem, GuideProgress)
- ✅ EF Core 7 integration with SQLite
- ✅ InstallVibeDbContext with fluent API configuration
- ✅ Repository pattern (IGuideRepository, GuideRepository)
- ✅ Cascade delete configuration (Guide → Steps → Media)
- ✅ JSON value conversion for CompletedStepIds
- ✅ Seeded sample HVAC guide with 3 steps and media
- ✅ Per-user progress tracking
- ✅ Comprehensive test examples

## Database

**Location:** `%LocalAppData%\InstallVibe\installvibe.db`

**Schemas:**
- **Authentication:** [STEP2_DATABASE_SCHEMA.md](STEP2_DATABASE_SCHEMA.md)
- **Guides & Progress:** [STEP3_DATABASE_SCHEMA.md](STEP3_DATABASE_SCHEMA.md)

### Default Credentials

| Username | Password | Role |
|----------|----------|------|
| `admin` | `admin123` | Admin |

⚠️ **Change the admin password in production!**

## Current Functionality

1. **Authentication:**
   - Secure login with PBKDF2 password hashing
   - Auto-seeded admin account
   - Case-insensitive username lookup
   - Session management (current user tracking)

2. **Guide Management:**
   - EF Core repository for CRUD operations
   - Sample HVAC installation guide with 3 steps
   - Media attachments (images/videos) per step
   - Per-user progress tracking with completion status

3. **Login View:**
   - Username and password input fields
   - Async authentication with loading state
   - Error message display for invalid credentials
   - Placeholder Register button

4. **Home View:**
   - Personalized welcome message with username and role
   - Logout functionality
   - Top navigation bar

## Seeded Sample Data

### Standard HVAC Unit Installation Guide
- **Category:** HVAC
- **Duration:** 180 minutes
- **Steps:**
  1. Pre-installation Safety Check (with safety image)
  2. Mounting the Unit
  3. Electrical Connection

Each step includes detailed instructions, required tools, and safety notes.

## Testing

- **Authentication:** [STEP2_TEST_SNIPPETS.md](STEP2_TEST_SNIPPETS.md)
- **Repository & EF Core:** [STEP3_TEST_SNIPPETS.md](STEP3_TEST_SNIPPETS.md)

## Next Steps

Future steps will add:
- User registration UI and workflow
- User management (add/edit/delete technician accounts)
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
- **Database:** SQLite
  - Microsoft.Data.Sqlite 7.0.10 (for auth)
  - Microsoft.EntityFrameworkCore.Sqlite 7.0.11 (for guides)
- **ORM:** Entity Framework Core 7.0.11
- **Password Hashing:** PBKDF2 with SHA256
- **Platform:** Windows App SDK 1.3
- **Deployment:** Unpackaged, self-contained

## Security Features

- **PBKDF2 Password Hashing:** 100,000 iterations with SHA256 and 32-byte random salt
- **Constant-Time Comparison:** Prevents timing attacks during password verification
- **Parameterized Queries:** SQL injection protection
- **Case-Insensitive Usernames:** Prevents duplicate accounts with different casing

## Notes

- **Step 1 (Complete):** Project scaffold with MVVM and navigation
- **Step 2 (Complete):** Authentication with SQLite database and PBKDF2 hashing
- **Step 3 (Complete):** EF Core models, repository pattern, and sample HVAC guide
- Database is automatically created on first run at `%LocalAppData%\InstallVibe\installvibe.db`
- Admin account (`admin`/`admin123`) and sample HVAC guide are auto-seeded
- EF Core handles database creation with `EnsureCreatedAsync()` - no migrations needed
- The application uses modern Windows design patterns and unpackaged deployment
