using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using InstallVibe.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace InstallVibe;

public sealed partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private bool _canGoBack;
    private string _currentPageTitle = "InstallVibe";

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool CanGoBack
    {
        get => _canGoBack;
        set
        {
            if (_canGoBack != value)
            {
                _canGoBack = value;
                OnPropertyChanged();
            }
        }
    }

    public string CurrentPageTitle
    {
        get => _currentPageTitle;
        set
        {
            if (_currentPageTitle != value)
            {
                _currentPageTitle = value;
                OnPropertyChanged();
            }
        }
    }

    public MainWindow(INavigationService navigationService, IAuthService authService)
    {
        _navigationService = navigationService;
        _authService = authService;

        InitializeComponent();

        // Set the navigation frame
        navigationService.Frame = ContentFrame;

        // Subscribe to navigation events
        ContentFrame.Navigated += OnNavigated;

        // Set admin visibility
        UpdateAdminVisibility();

        // Set window size
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));

        // Center the window
        CenterWindow();
    }

    private void UpdateAdminVisibility()
    {
        AdminMenuItem.Visibility = _authService.IsAdmin() ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        CanGoBack = ContentFrame.CanGoBack;

        // Update page title based on current page
        if (ContentFrame.Content != null)
        {
            var pageType = ContentFrame.Content.GetType();
            CurrentPageTitle = pageType.Name switch
            {
                "GuideLibraryView" => "Guide Library",
                "GuideDetailView" => "Guide Details",
                "GuideViewerView" => "Guide Viewer",
                "StepViewerView" => "Step Viewer",
                "AdminDashboardView" => "Admin Dashboard",
                "GuideEditorView" => "Guide Editor",
                "StepEditorView" => "Step Editor",
                "SettingsView" => "Settings",
                "HomeView" => "Home",
                _ => "InstallVibe"
            };
        }

        // Update selected navigation item
        UpdateNavigationSelection();
    }

    private void UpdateNavigationSelection()
    {
        var currentPage = ContentFrame.Content?.GetType().Name;

        // First, deselect all menu items
        foreach (var item in NavView.MenuItems)
        {
            if (item is NavigationViewItem navItem)
            {
                navItem.IsSelected = false;
            }
        }

        // Then select the appropriate item based on current page
        if (currentPage == "SettingsView")
        {
            NavView.SelectedItem = NavView.SettingsItem;
        }
        else
        {
            foreach (var item in NavView.MenuItems)
            {
                if (item is NavigationViewItem navItem)
                {
                    var tag = navItem.Tag?.ToString();
                    var shouldSelect = tag switch
                    {
                        "GuideLibrary" => currentPage == "GuideLibraryView" || currentPage == "GuideDetailView",
                        "AdminDashboard" => currentPage == "AdminDashboardView" ||
                                           currentPage == "GuideEditorView" ||
                                           currentPage == "StepEditorView",
                        _ => false
                    };

                    if (shouldSelect)
                    {
                        navItem.IsSelected = true;
                        NavView.SelectedItem = navItem;
                        break;
                    }
                }
            }
        }
    }

    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        _navigationService.GoBack();
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            _navigationService.NavigateToSettings();
            return;
        }

        if (args.InvokedItemContainer is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();

            switch (tag)
            {
                case "GuideLibrary":
                    _navigationService.NavigateToGuideLibrary();
                    break;
                case "InProgress":
                    // TODO: Implement in-progress guides view
                    _navigationService.NavigateToGuideLibrary();
                    break;
                case "Completed":
                    // TODO: Implement completed guides view
                    _navigationService.NavigateToGuideLibrary();
                    break;
                case "AdminDashboard":
                    _navigationService.NavigateToAdminDashboard();
                    break;
            }
        }
    }

    private void CenterWindow()
    {
        var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(
            AppWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);

        if (displayArea != null)
        {
            var centeredPosition = AppWindow.Position;
            centeredPosition.X = (displayArea.WorkArea.Width - AppWindow.Size.Width) / 2;
            centeredPosition.Y = (displayArea.WorkArea.Height - AppWindow.Size.Height) / 2;
            AppWindow.Move(centeredPosition);
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
