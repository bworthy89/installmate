# InstallVibe UI Tests

Automated UI tests using WinAppDriver for InstallVibe WinUI 3 application.

## Prerequisites

### 1. Install WinAppDriver

Download and install WinAppDriver from:
https://github.com/microsoft/WinAppDriver/releases

Install to default location: `C:\Program Files (x86)\Windows Application Driver\`

### 2. Enable Developer Mode

1. Open Windows Settings
2. Go to **Update & Security** > **For developers**
3. Enable **Developer Mode**

### 3. Install InstallVibe

The application must be installed as an MSIX package to run UI tests.

```powershell
# Install from package
Add-AppxPackage -Path "path\to\InstallVibe.msix"
```

## Running Tests

### 1. Start WinAppDriver

Open an **Administrator** PowerShell window:

```powershell
cd "C:\Program Files (x86)\Windows Application Driver"
.\WinAppDriver.exe
```

Leave this window open while running tests.

### 2. Run Tests

In a separate terminal:

```powershell
# Run all UI tests
dotnet test InstallVibe.UITests

# Run specific test class
dotnet test --filter "FullyQualifiedName~LoginTests"

# Run specific test
dotnet test --filter "FullyQualifiedName~LoginTests.Login_WithValidCredentials_Succeeds"

# Run with verbose output
dotnet test -v normal
```

## Test Structure

```
InstallVibe.UITests/
├── TestBase.cs                 # Base class for all UI tests
├── LoginTests.cs              # Login functionality tests
├── GuideNavigationTests.cs    # Guide navigation and workflow tests
├── AccessibilityTests.cs      # WCAG 2.1 compliance tests
└── README.md                  # This file
```

## Writing New Tests

1. **Inherit from TestBase**
   ```csharp
   public class MyTests : TestBase
   {
       [Fact]
       public void MyTest()
       {
           // Test code
       }
   }
   ```

2. **Use Accessibility IDs**

   In XAML, set AutomationProperties.AutomationId:
   ```xml
   <Button x:Name="LoginButton"
           AutomationProperties.AutomationId="LoginButton"
           Content="Login" />
   ```

   In test code:
   ```csharp
   var button = FindElementByAccessibilityId("LoginButton");
   button.Click();
   ```

3. **Wait for Elements**
   ```csharp
   WaitForElement("ElementId", timeoutSeconds: 10);
   ```

## Finding the App ID

If you need to update the App ID:

```powershell
# List all installed apps
Get-AppxPackage | Where-Object { $_.Name -like "*InstallVibe*" } | Select Name, PackageFamilyName
```

Use the PackageFamilyName as the App ID in TestBase.cs.

## Troubleshooting

### WinAppDriver not starting
- Ensure you're running as Administrator
- Check port 4723 is not in use: `netstat -ano | findstr :4723`

### Tests failing with "Application not found"
- Verify InstallVibe is installed: `Get-AppxPackage *InstallVibe*`
- Check the App ID in TestBase.cs matches your installation

### Element not found errors
- Verify AutomationProperties.AutomationId is set in XAML
- Use Inspect.exe (Windows SDK) to view accessibility tree
- Increase timeout in WaitForElement()

### Slow test execution
- UI tests are inherently slower than unit tests
- Run in parallel with caution (may cause timing issues)
- Consider running critical path tests only in CI/CD

## CI/CD Integration

### GitHub Actions Example

```yaml
- name: Install WinAppDriver
  run: |
    choco install winappdriver

- name: Start WinAppDriver
  run: |
    Start-Process "C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe"
    Start-Sleep -Seconds 5

- name: Install InstallVibe
  run: |
    Add-AppxPackage -Path "${{ github.workspace }}\output\InstallVibe.msix"

- name: Run UI Tests
  run: |
    dotnet test InstallVibe.UITests --logger "trx;LogFileName=uitest-results.trx"
```

## Best Practices

1. **Keep tests independent** - Each test should set up its own state
2. **Use meaningful test names** - Follow pattern: `MethodName_Scenario_ExpectedBehavior`
3. **Clean up after tests** - TestBase.Dispose() handles app cleanup
4. **Use explicit waits** - Don't rely on implicit waits for critical elements
5. **Test accessibility** - Verify screen reader support, keyboard navigation, contrast
6. **Run locally first** - Always verify tests pass locally before pushing

## Coverage Goals

- ✅ **Critical Path**: 100% coverage (login, guide selection, step navigation)
- ✅ **Error Handling**: Test invalid inputs, network failures
- ✅ **Accessibility**: WCAG 2.1 Level AA compliance
- ⚠️ **Edge Cases**: Offline mode, concurrent users, data corruption

## Resources

- [WinAppDriver Documentation](https://github.com/microsoft/WinAppDriver)
- [Appium Documentation](https://appium.io/docs/en/about-appium/intro/)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [UI Automation in WinUI 3](https://docs.microsoft.com/en-us/windows/apps/develop/ui-input/accessibility)
