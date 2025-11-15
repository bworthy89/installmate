using Xunit;
using FluentAssertions;
using OpenQA.Selenium.Appium.Windows;
using System.Linq;

namespace InstallVibe.UITests;

public class GuideNavigationTests : TestBase
{
    public GuideNavigationTests()
    {
        // Login first
        var usernameBox = FindElementByAccessibilityId("UsernameTextBox");
        var passwordBox = FindElementByAccessibilityId("PasswordTextBox");
        var loginButton = FindElementByAccessibilityId("LoginButton");

        usernameBox.SendKeys("tech001");
        passwordBox.SendKeys("Password123!");
        loginButton.Click();

        WaitForElement("GuideLibraryPage", timeoutSeconds: 10);
    }

    [Fact]
    public void GuideLibrary_DisplaysGuides()
    {
        // Act
        var guideList = FindElementByAccessibilityId("GuideListView");

        // Assert
        guideList.Should().NotBeNull();
        var guideItems = Driver.FindElementsByClassName("ListViewItem");
        guideItems.Should().NotBeEmpty();
    }

    [Fact]
    public void SearchBox_FiltersGuides()
    {
        // Arrange
        var searchBox = FindElementByAccessibilityId("SearchTextBox");
        var guideList = FindElementByAccessibilityId("GuideListView");

        // Act
        searchBox.SendKeys("Motor");
        System.Threading.Thread.Sleep(1000); // Allow filter to apply

        // Assert
        var filteredItems = Driver.FindElementsByClassName("ListViewItem");
        filteredItems.Should().NotBeEmpty();
        // All visible items should contain "Motor" in their text
        foreach (var item in filteredItems)
        {
            item.Text.Should().ContainAny("Motor", "motor");
        }
    }

    [Fact]
    public void SelectGuide_NavigatesToStepView()
    {
        // Arrange
        var guideItems = Driver.FindElementsByClassName("ListViewItem");
        guideItems.Should().NotBeEmpty();

        // Act
        guideItems.First().Click();

        // Assert
        WaitForElement("StepViewPage", timeoutSeconds: 5);
        var stepTitle = FindElementByAccessibilityId("StepTitleTextBlock");
        stepTitle.Should().NotBeNull();
        stepTitle.Text.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void StepNavigation_NextButtonWorks()
    {
        // Arrange - Navigate to a guide
        var guideItems = Driver.FindElementsByClassName("ListViewItem");
        guideItems.First().Click();
        WaitForElement("StepViewPage", timeoutSeconds: 5);

        var stepTitle = FindElementByAccessibilityId("StepTitleTextBlock");
        var initialStepTitle = stepTitle.Text;

        // Act
        var nextButton = FindElementByAccessibilityId("NextStepButton");
        nextButton.Click();
        System.Threading.Thread.Sleep(1000); // Allow navigation

        // Assert
        var newStepTitle = FindElementByAccessibilityId("StepTitleTextBlock");
        newStepTitle.Text.Should().NotBe(initialStepTitle);
    }

    [Fact]
    public void StepNavigation_PreviousButtonWorks()
    {
        // Arrange - Navigate to a guide and go to step 2
        var guideItems = Driver.FindElementsByClassName("ListViewItem");
        guideItems.First().Click();
        WaitForElement("StepViewPage", timeoutSeconds: 5);

        var nextButton = FindElementByAccessibilityId("NextStepButton");
        nextButton.Click();
        System.Threading.Thread.Sleep(1000);

        var stepTitle = FindElementByAccessibilityId("StepTitleTextBlock");
        var step2Title = stepTitle.Text;

        // Act
        var previousButton = FindElementByAccessibilityId("PreviousStepButton");
        previousButton.Click();
        System.Threading.Thread.Sleep(1000);

        // Assert
        var newStepTitle = FindElementByAccessibilityId("StepTitleTextBlock");
        newStepTitle.Text.Should().NotBe(step2Title);
    }

    [Fact]
    public void MarkStepComplete_UpdatesProgress()
    {
        // Arrange - Navigate to a guide
        var guideItems = Driver.FindElementsByClassName("ListViewItem");
        guideItems.First().Click();
        WaitForElement("StepViewPage", timeoutSeconds: 5);

        // Act
        var completeButton = FindElementByAccessibilityId("MarkCompleteButton");
        completeButton.Click();

        // Assert
        var progressBar = FindElementByAccessibilityId("GuideProgressBar");
        var progressValue = double.Parse(progressBar.GetAttribute("Value"));
        progressValue.Should().BeGreaterThan(0);
    }

    [Fact]
    public void BackButton_ReturnsToGuideLibrary()
    {
        // Arrange - Navigate to a guide
        var guideItems = Driver.FindElementsByClassName("ListViewItem");
        guideItems.First().Click();
        WaitForElement("StepViewPage", timeoutSeconds: 5);

        // Act
        var backButton = FindElementByAccessibilityId("BackButton");
        backButton.Click();

        // Assert
        WaitForElement("GuideLibraryPage", timeoutSeconds: 5);
        var guideList = FindElementByAccessibilityId("GuideListView");
        guideList.Should().NotBeNull();
    }
}
