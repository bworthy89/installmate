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
        System.Diagnostics.Debug.WriteLine($"NavigateTo called for page type: {pageType.Name}");

        if (_frame == null)
        {
            System.Diagnostics.Debug.WriteLine("ERROR: Navigation frame is null!");
            throw new InvalidOperationException("Navigation frame is not set.");
        }

        // Store parameters for the page type
        if (parameters != null && parameters.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"Storing {parameters.Count} parameters");
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
            System.Diagnostics.Debug.WriteLine($"ERROR: Page {pageType.Name} not found in DI container!");
            throw new InvalidOperationException($"Page {pageType.Name} is not registered in the DI container.");
        }

        System.Diagnostics.Debug.WriteLine($"Page instance created: {page.GetType().Name}");

        // If the page's DataContext has an Initialize method, call it with parameters
        if (page is FrameworkElement element && element.DataContext != null)
        {
            var dataContext = element.DataContext;
            var initializeMethod = dataContext.GetType().GetMethod("Initialize");
            if (initializeMethod != null && parameters != null)
            {
                System.Diagnostics.Debug.WriteLine("Calling Initialize method on DataContext");
                initializeMethod.Invoke(dataContext, new object[] { parameters });
            }
        }

        _frame.Content = page;
        System.Diagnostics.Debug.WriteLine("Navigation complete - frame content set");
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
        System.Diagnostics.Debug.WriteLine($"NavigateToGuideDetail called with GuideId: {guideId}");
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

    public void NavigateToGuideViewer(int guideId)
    {
        System.Diagnostics.Debug.WriteLine($"NavigateToGuideViewer called with GuideId: {guideId}");
        var parameters = new Dictionary<string, object>
        {
            { "GuideId", guideId }
        };
        NavigateTo<Views.GuideViewerView>(parameters);
    }

    public Dictionary<string, object>? GetNavigationParameters(Type pageType)
    {
        return _navigationParameters.TryGetValue(pageType, out var parameters) ? parameters : null;
    }

    // Admin navigation methods
    public void NavigateToAdminDashboard()
    {
        NavigateTo<Views.Admin.AdminDashboardView>();
    }

    public void NavigateToGuideEditor(int? guideId = null)
    {
        if (guideId.HasValue)
        {
            var parameters = new Dictionary<string, object>
            {
                { "GuideId", guideId.Value }
            };
            NavigateTo<Views.Admin.GuideEditorView>(parameters);
        }
        else
        {
            NavigateTo<Views.Admin.GuideEditorView>();
        }
    }

    public void NavigateToStepEditor(int stepId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "stepId", stepId }
        };
        NavigateTo<Views.Admin.StepEditorView>(parameters);
    }
}
