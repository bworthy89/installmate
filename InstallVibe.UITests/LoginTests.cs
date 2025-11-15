using Xunit;
using FluentAssertions;
using OpenQA.Selenium.Appium.Windows;

namespace InstallVibe.UITests;

public class LoginTests : TestBase
{
    [Fact]
    public void LoginPage_DisplaysCorrectly()
    {
        // Act
        var usernameBox = FindElementByAccessibilityId("UsernameTextBox");
        var passwordBox = FindElementByAccessibilityId("PasswordTextBox");
        var loginButton = FindElementByAccessibilityId("LoginButton");

        // Assert
        usernameBox.Should().NotBeNull();
        passwordBox.Should().NotBeNull();
        loginButton.Should().NotBeNull();
        loginButton.Enabled.Should().BeFalse(); // Initially disabled
    }

    [Fact]
    public void Login_WithValidCredentials_Succeeds()
    {
        // Arrange
        var usernameBox = FindElementByAccessibilityId("UsernameTextBox");
        var passwordBox = FindElementByAccessibilityId("PasswordTextBox");
        var loginButton = FindElementByAccessibilityId("LoginButton");

        // Act
        usernameBox.SendKeys("tech001");
        passwordBox.SendKeys("Password123!");
        loginButton.Click();

        // Assert
        WaitForElement("GuideLibraryPage", timeoutSeconds: 10);
        var libraryTitle = FindElementByName("Guide Library");
        libraryTitle.Should().NotBeNull();
    }

    [Fact]
    public void Login_WithInvalidCredentials_ShowsError()
    {
        // Arrange
        var usernameBox = FindElementByAccessibilityId("UsernameTextBox");
        var passwordBox = FindElementByAccessibilityId("PasswordTextBox");
        var loginButton = FindElementByAccessibilityId("LoginButton");

        // Act
        usernameBox.SendKeys("invaliduser");
        passwordBox.SendKeys("wrongpassword");
        loginButton.Click();

        // Assert
        WaitForElement("ErrorMessageTextBlock", timeoutSeconds: 5);
        var errorMessage = FindElementByAccessibilityId("ErrorMessageTextBlock");
        errorMessage.Text.Should().Contain("Invalid");
    }

    [Fact]
    public void LoginButton_EnabledWhenBothFieldsFilled()
    {
        // Arrange
        var usernameBox = FindElementByAccessibilityId("UsernameTextBox");
        var passwordBox = FindElementByAccessibilityId("PasswordTextBox");
        var loginButton = FindElementByAccessibilityId("LoginButton");

        // Act - Initially disabled
        loginButton.Enabled.Should().BeFalse();

        // Act - Fill username only
        usernameBox.SendKeys("tech001");
        loginButton.Enabled.Should().BeFalse();

        // Act - Fill password
        passwordBox.SendKeys("password");
        System.Threading.Thread.Sleep(500); // Allow binding update

        // Assert
        loginButton.Enabled.Should().BeTrue();
    }

    [Fact]
    public void PasswordBox_MasksInput()
    {
        // Arrange
        var passwordBox = FindElementByAccessibilityId("PasswordTextBox");

        // Act
        passwordBox.SendKeys("SecretPassword");

        // Assert
        // Password box should not display plain text
        passwordBox.GetAttribute("Value.IsPassword").Should().Be("True");
    }
}
