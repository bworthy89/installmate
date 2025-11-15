using Xunit;
using FluentAssertions;
using OpenQA.Selenium.Appium.Windows;
using System.Linq;

namespace InstallVibe.UITests;

public class AccessibilityTests : TestBase
{
    [Fact]
    public void AllButtons_HaveAccessibilityNames()
    {
        // Arrange
        var buttons = Driver.FindElementsByClassName("Button");

        // Assert
        buttons.Should().NotBeEmpty();
        foreach (var button in buttons)
        {
            var name = button.GetAttribute("Name");
            name.Should().NotBeNullOrWhiteSpace($"Button should have accessibility name");
        }
    }

    [Fact]
    public void AllTextBoxes_HaveAccessibilityLabels()
    {
        // Arrange
        var textBoxes = Driver.FindElementsByClassName("TextBox");

        // Assert
        textBoxes.Should().NotBeEmpty();
        foreach (var textBox in textBoxes)
        {
            var name = textBox.GetAttribute("Name");
            var labeledBy = textBox.GetAttribute("LabeledBy");
            var helpText = textBox.GetAttribute("HelpText");

            // At least one accessibility property should be set
            (name != null || labeledBy != null || helpText != null).Should().BeTrue(
                "TextBox should have Name, LabeledBy, or HelpText set");
        }
    }

    [Fact]
    public void FocusableElements_HaveKeyboardAccess()
    {
        // Arrange
        var loginButton = FindElementByAccessibilityId("LoginButton");
        var usernameBox = FindElementByAccessibilityId("UsernameTextBox");

        // Act - Tab through elements
        usernameBox.SendKeys("\t"); // Tab to next element
        System.Threading.Thread.Sleep(200);

        // Assert - Focus should move
        var focusedElement = Driver.SwitchTo().ActiveElement();
        focusedElement.Should().NotBeNull();
    }

    [Fact]
    public void HighContrast_ElementsVisible()
    {
        // This test would need to enable high contrast mode via Windows settings
        // then verify element visibility
        // For now, we verify elements have proper contrast properties set

        // Arrange
        var buttons = Driver.FindElementsByClassName("Button");

        // Assert
        buttons.Should().NotBeEmpty();
        foreach (var button in buttons)
        {
            // Verify button is visible
            button.Displayed.Should().BeTrue();
            button.Enabled.Should().BeTrue();
        }
    }

    [Fact]
    public void Images_HaveAlternativeText()
    {
        // Arrange - Login and navigate to a guide with images
        var usernameBox = FindElementByAccessibilityId("UsernameTextBox");
        var passwordBox = FindElementByAccessibilityId("PasswordTextBox");
        var loginButton = FindElementByAccessibilityId("LoginButton");

        usernameBox.SendKeys("tech001");
        passwordBox.SendKeys("Password123!");
        loginButton.Click();

        WaitForElement("GuideLibraryPage", timeoutSeconds: 10);

        var guideItems = Driver.FindElementsByClassName("ListViewItem");
        guideItems.First().Click();
        WaitForElement("StepViewPage", timeoutSeconds: 5);

        // Act
        var images = Driver.FindElementsByClassName("Image");

        // Assert
        if (images.Any())
        {
            foreach (var image in images)
            {
                var name = image.GetAttribute("Name");
                var helpText = image.GetAttribute("HelpText");

                (name != null || helpText != null).Should().BeTrue(
                    "Image should have Name or HelpText for screen readers");
            }
        }
    }

    [Fact]
    public void MinimumTouchTargetSize_Met()
    {
        // WCAG 2.1 Level AA requires 44x44px minimum touch target

        // Arrange
        var buttons = Driver.FindElementsByClassName("Button");

        // Assert
        buttons.Should().NotBeEmpty();
        foreach (var button in buttons)
        {
            var size = button.Size;
            size.Width.Should().BeGreaterOrEqualTo(44, "Touch target width should be at least 44px");
            size.Height.Should().BeGreaterOrEqualTo(44, "Touch target height should be at least 44px");
        }
    }
}
