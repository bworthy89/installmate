using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace InstallVibe.Services;

public class NavigationService : INavigationService
{
    private Frame? _frame;
    private readonly Dictionary<Type, Dictionary<string, object>> _navigationParameters = new();

    public Frame? Frame
    {
        get => _frame;
        set => _frame = value;
    }

    public bool CanGoBack => _frame?.CanGoBack ?? false;

    public void NavigateTo<T>() where T : class
    {
        NavigateTo(typeof(T), null);
    }

    public void NavigateTo<T>(Dictionary<string, object> parameters) where T : class
    {
        NavigateTo(typeof(T), parameters);
    }

    public void NavigateTo(Type pageType)
    {
        NavigateTo(pageType, null);
    }

    public void NavigateTo(Type pageType, Dictionary<string, object>? parameters)
    {
        if (_frame == null)
        {
            throw new InvalidOperationException("Navigation frame is not set.");
        }

        // Store parameters for the page type
        if (parameters != null && parameters.Count > 0)
        {
            _navigationParameters[pageType] = parameters;
        }
        else if (_navigationParameters.ContainsKey(pageType))
        {
            _navigationParameters.Remove(pageType);
        }

        // Get the page instance from DI container (creates new scope for transient)
        using var scope = App.Services.CreateScope();
        var page = scope.ServiceProvider.GetService(pageType);

        if (page == null)
        {
            throw new InvalidOperationException($"Page {pageType.Name} is not registered in the DI container.");
        }

        // If the page's DataContext has an Initialize method, call it with parameters
        if (page is FrameworkElement element && element.DataContext != null)
        {
            var dataContext = element.DataContext;
            var initializeMethod = dataContext.GetType().GetMethod("Initialize");
            if (initializeMethod != null && parameters != null)
            {
                initializeMethod.Invoke(dataContext, new object[] { parameters });
            }
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

    // Strongly-typed navigation methods
    public void NavigateToGuideLibrary()
    {
        NavigateTo<Views.GuideLibraryView>();
    }

    public void NavigateToGuideDetail(int guideId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "GuideId", guideId }
        };
        NavigateTo<Views.GuideDetailView>(parameters);
    }

    public void NavigateToStepViewer(int guideId, int stepIndex = 0)
    {
        var parameters = new Dictionary<string, object>
        {
            { "GuideId", guideId },
            { "StepIndex", stepIndex }
        };
        NavigateTo<Views.StepViewerView>(parameters);
    }

    public Dictionary<string, object>? GetNavigationParameters(Type pageType)
    {
        return _navigationParameters.TryGetValue(pageType, out var parameters) ? parameters : null;
    }
}
