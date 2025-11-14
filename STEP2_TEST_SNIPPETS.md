# Step 2: Test Snippets

## Authentication Service Test Examples

### 1. Basic Login Test

```csharp
using InstallVibe.Services;
using InstallVibe.Models;

// Get services from DI container
var authService = App.Services.GetRequiredService<IAuthService>();

// Test login with seeded admin account
var result = await authService.Login("admin", "admin123");

if (result != null)
{
    Console.WriteLine($"Login successful!");
    Console.WriteLine($"User ID: {result.Id}");
    Console.WriteLine($"Username: {result.Username}");
    Console.WriteLine($"Role: {result.Role}");
    // Expected output:
    // Login successful!
    // User ID: 1
    // Username: admin
    // Role: Admin
}
else
{
    Console.WriteLine("Login failed!");
}
```

### 2. Failed Login Test

```csharp
var authService = App.Services.GetRequiredService<IAuthService>();

// Test with invalid credentials
var result = await authService.Login("admin", "wrongpassword");

if (result == null)
{
    Console.WriteLine("Login correctly rejected invalid password");
    // Expected: Login correctly rejected invalid password
}
```

### 3. Get Current User Test

```csharp
var authService = App.Services.GetRequiredService<IAuthService>();

// Login first
var loginResult = await authService.Login("admin", "admin123");

// Get current user
var currentUser = authService.GetCurrentUser();

if (currentUser != null)
{
    Console.WriteLine($"Current user: {currentUser.Username} ({currentUser.Role})");
    // Expected: Current user: admin (Admin)
}
```

### 4. Logout Test

```csharp
var authService = App.Services.GetRequiredService<IAuthService>();

// Login
await authService.Login("admin", "admin123");
Console.WriteLine($"Before logout: {authService.GetCurrentUser()?.Username}");
// Expected: Before logout: admin

// Logout
await authService.Logout();
Console.WriteLine($"After logout: {authService.GetCurrentUser()?.Username ?? "null"}");
// Expected: After logout: null
```

### 5. Role-Based Navigation Test

```csharp
var authService = App.Services.GetRequiredService<IAuthService>();
var navigationService = App.Services.GetRequiredService<INavigationService>();

var user = await authService.Login("admin", "admin123");

if (user != null)
{
    switch (user.Role)
    {
        case UserRole.Admin:
            Console.WriteLine("Navigating to Admin Home");
            navigationService.NavigateTo<HomeView>();
            break;
        case UserRole.Technician:
            Console.WriteLine("Navigating to Technician Home");
            navigationService.NavigateTo<HomeView>();
            break;
    }
}
```

### 6. Password Hash Verification Test

```csharp
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

// Direct database test to verify PBKDF2 hashing
var dbService = App.Services.GetRequiredService<IDatabaseService>();
using var connection = new SqliteConnection(dbService.GetConnectionString());
await connection.OpenAsync();

var query = "SELECT PasswordHash FROM Users WHERE Username = 'admin'";
using var command = new SqliteCommand(query, connection);
var hash = (string?)await command.ExecuteScalarAsync();

if (hash != null)
{
    var parts = hash.Split(':');
    Console.WriteLine($"Salt (base64): {parts[0]}");
    Console.WriteLine($"Hash (base64): {parts[1]}");
    Console.WriteLine($"Salt length: {Convert.FromBase64String(parts[0]).Length} bytes");
    Console.WriteLine($"Hash length: {Convert.FromBase64String(parts[1]).Length} bytes");

    // Expected output:
    // Salt (base64): [random base64 string]
    // Hash (base64): [random base64 string]
    // Salt length: 32 bytes
    // Hash length: 32 bytes
}
```

### 7. Case-Insensitive Username Test

```csharp
var authService = App.Services.GetRequiredService<IAuthService>();

// Test case-insensitive login
var result1 = await authService.Login("admin", "admin123");
var result2 = await authService.Login("ADMIN", "admin123");
var result3 = await authService.Login("AdMiN", "admin123");

Console.WriteLine($"'admin' login: {result1 != null}");
Console.WriteLine($"'ADMIN' login: {result2 != null}");
Console.WriteLine($"'AdMiN' login: {result3 != null}");

// Expected output:
// 'admin' login: True
// 'ADMIN' login: True
// 'AdMiN' login: True
```

## Running Tests in Visual Studio

### Option 1: Immediate Window (During Debug)

1. Set a breakpoint in `HomeViewModel` constructor
2. Run the app (F5) and login
3. Open **Debug → Windows → Immediate**
4. Type test code directly

### Option 2: Create a Test Method

Add to `HomeViewModel.cs`:

```csharp
[RelayCommand]
private async Task RunTests()
{
    var authService = _authService;

    // Test 1: Login
    var result = await authService.Login("admin", "admin123");
    System.Diagnostics.Debug.WriteLine($"Login test: {result?.Username} ({result?.Role})");

    // Test 2: Get current user
    var current = authService.GetCurrentUser();
    System.Diagnostics.Debug.WriteLine($"Current user: {current?.Username}");
}
```

Then add a button in `HomeView.xaml`:

```xml
<Button Content="Run Tests" Command="{x:Bind ViewModel.RunTestsCommand}" />
```

Check the **Output** window in Visual Studio to see debug output.

## Expected Behavior Summary

✅ **Admin account exists** after first run
✅ **Login with "admin" / "admin123"** succeeds
✅ **Wrong password** is rejected
✅ **Case variations** of username work (admin, ADMIN, AdMiN)
✅ **Current user** is available after login
✅ **Logout** clears current user
✅ **Navigation** works based on user role
✅ **Password hashes** use PBKDF2 with salt
