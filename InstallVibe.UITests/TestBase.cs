using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;

namespace InstallVibe.UITests;

public class TestBase : IDisposable
{
    protected WindowsDriver<WindowsElement> Driver { get; private set; }
    protected const string AppId = "InstallVibe_8wekyb3d8bbwe!App";
    protected const string WinAppDriverUrl = "http://127.0.0.1:4723";

    public TestBase()
    {
        var appiumOptions = new AppiumOptions();
        appiumOptions.AddAdditionalCapability("app", AppId);
        appiumOptions.AddAdditionalCapability("deviceName", "WindowsPC");
        appiumOptions.AddAdditionalCapability("platformName", "Windows");

        Driver = new WindowsDriver<WindowsElement>(new Uri(WinAppDriverUrl), appiumOptions);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    public void Dispose()
    {
        Driver?.Quit();
        Driver?.Dispose();
    }

    protected WindowsElement FindElementByAccessibilityId(string id)
    {
        return Driver.FindElementByAccessibilityId(id);
    }

    protected WindowsElement FindElementByName(string name)
    {
        return Driver.FindElementByName(name);
    }

    protected void WaitForElement(string accessibilityId, int timeoutSeconds = 10)
    {
        var timeout = DateTime.Now.AddSeconds(timeoutSeconds);
        while (DateTime.Now < timeout)
        {
            try
            {
                var element = Driver.FindElementByAccessibilityId(accessibilityId);
                if (element.Displayed)
                    return;
            }
            catch
            {
                System.Threading.Thread.Sleep(500);
            }
        }
        throw new Exception($"Element with ID '{accessibilityId}' not found within {timeoutSeconds} seconds");
    }
}
