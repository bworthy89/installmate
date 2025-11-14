using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;

namespace InstallVibe.Services;

public class NavigationService : INavigationService
{
    private Frame? _frame;

    public Frame? Frame
    {
        get => _frame;
        set => _frame = value;
    }

    public bool CanGoBack => _frame?.CanGoBack ?? false;

    public void NavigateTo<T>() where T : class
    {
        NavigateTo(typeof(T));
    }

    public void NavigateTo(Type pageType)
    {
        if (_frame == null)
        {
            throw new InvalidOperationException("Navigation frame is not set.");
        }

        // Get the page instance from DI container
        var page = App.Services.GetService(pageType);

        if (page == null)
        {
            throw new InvalidOperationException($"Page {pageType.Name} is not registered in the DI container.");
        }

        _frame.Content = page;
    }

    public void GoBack()
    {
        if (_frame?.CanGoBack == true)
        {
            _frame.GoBack();
        }
    }
}
