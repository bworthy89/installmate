using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace InstallVibe.Services;

public interface INavigationService
{
    Frame? Frame { get; set; }
    bool CanGoBack { get; }

    void NavigateTo<T>() where T : class;
    void NavigateTo<T>(Dictionary<string, object> parameters) where T : class;
    void NavigateTo(Type pageType);
    void NavigateTo(Type pageType, Dictionary<string, object> parameters);
    void GoBack();

    // Navigation parameter retrieval
    Dictionary<string, object>? GetNavigationParameters(Type pageType);

    // Strongly-typed navigation methods
    void NavigateToGuideLibrary();
    void NavigateToGuideDetail(int guideId);
    void NavigateToStepViewer(int guideId, int stepIndex = 0);
    void NavigateToGuideViewer(int guideId);

    // Admin navigation methods
    void NavigateToAdminDashboard();
    void NavigateToGuideEditor(int? guideId = null);
    void NavigateToStepEditor(int stepId);

    // Settings navigation
    void NavigateToSettings();
}
