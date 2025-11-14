using Microsoft.UI.Xaml.Controls;
using System;

namespace InstallVibe.Services;

public interface INavigationService
{
    Frame? Frame { get; set; }
    bool CanGoBack { get; }

    void NavigateTo<T>() where T : class;
    void NavigateTo(Type pageType);
    void GoBack();
}
