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

        // Register keyboard accelerators
        RegisterKeyboardShortcuts();

        // Set window size
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));

        // Center the window
        CenterWindow();
    }

    private void UpdateAdminVisibility()
    {
        try
        {
            AdminMenuItem.Visibility = _authService.IsAdmin() ? Visibility.Visible : Visibility.Collapsed;
        }
        catch
        {
            // Hide admin menu if there's any error checking admin status (e.g., no user logged in yet)
            AdminMenuItem.Visibility = Visibility.Collapsed;
        }
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
                "LoginView" => "Login",
                _ => "InstallVibe"
            };
        }

        // Update admin menu visibility (in case user just logged in/out)
        UpdateAdminVisibility();

        // Update selected navigation item - dispatch to UI thread to avoid COM exceptions
        DispatcherQueue.TryEnqueue(() =>
        {
            UpdateNavigationSelection();
        });
    }

    private void UpdateNavigationSelection()
    {
        try
        {
            var currentPage = ContentFrame.Content?.GetType().Name;

            // Handle settings selection
            if (currentPage == "SettingsView")
            {
                if (NavView.SelectedItem != NavView.SettingsItem)
                {
                    NavView.SelectedItem = NavView.SettingsItem;
                }
                return;
            }

            // Find and select the appropriate menu item
            object? itemToSelect = null;

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
                        itemToSelect = navItem;
                        break;
                    }
                }
            }

            // Only update if selection needs to change
            if (itemToSelect != null && NavView.SelectedItem != itemToSelect)
            {
                NavView.SelectedItem = itemToSelect;
            }
            else if (itemToSelect == null && NavView.SelectedItem != null)
            {
                // Clear selection for pages that don't have a menu item (like LoginView, HomeView)
                NavView.SelectedItem = null;
            }
        }
        catch
        {
            // Silently fail if navigation selection update fails
            // This prevents navigation from breaking if there's a UI issue
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

    /// <summary>
    /// Registers keyboard shortcuts for improved accessibility and power-user workflows
    /// </summary>
    private void RegisterKeyboardShortcuts()
    {
        // Alt+Left: Go Back
        var goBackAccelerator = new Microsoft.UI.Xaml.Input.KeyboardAccelerator
        {
            Key = Windows.System.VirtualKey.Left,
            Modifiers = Windows.System.VirtualKeyModifiers.Menu // Alt key
        };
        goBackAccelerator.Invoked += (sender, args) =>
        {
            if (CanGoBack)
            {
                _navigationService.GoBack();
                args.Handled = true;
            }
        };
        RootGrid.KeyboardAccelerators.Add(goBackAccelerator);

        // Alt+Home: Navigate to Home/Guide Library
        var goHomeAccelerator = new Microsoft.UI.Xaml.Input.KeyboardAccelerator
        {
            Key = Windows.System.VirtualKey.Home,
            Modifiers = Windows.System.VirtualKeyModifiers.Menu
        };
        goHomeAccelerator.Invoked += (sender, args) =>
        {
            _navigationService.NavigateToGuideLibrary();
            args.Handled = true;
        };
        RootGrid.KeyboardAccelerators.Add(goHomeAccelerator);

        // Ctrl+Comma: Open Settings
        var settingsAccelerator = new Microsoft.UI.Xaml.Input.KeyboardAccelerator
        {
            Key = (Windows.System.VirtualKey)188, // VK_OEM_COMMA
            Modifiers = Windows.System.VirtualKeyModifiers.Control
        };
        settingsAccelerator.Invoked += (sender, args) =>
        {
            _navigationService.NavigateToSettings();
            args.Handled = true;
        };
        RootGrid.KeyboardAccelerators.Add(settingsAccelerator);

        // F1: Help (future implementation)
        var helpAccelerator = new Microsoft.UI.Xaml.Input.KeyboardAccelerator
        {
            Key = Windows.System.VirtualKey.F1
        };
        helpAccelerator.Invoked += (sender, args) =>
        {
            // TODO: Navigate to help documentation
            System.Diagnostics.Debug.WriteLine("F1 - Help requested");
            args.Handled = true;
        };
        RootGrid.KeyboardAccelerators.Add(helpAccelerator);

        // Ctrl+F: Search (future implementation)
        var searchAccelerator = new Microsoft.UI.Xaml.Input.KeyboardAccelerator
        {
            Key = Windows.System.VirtualKey.F,
            Modifiers = Windows.System.VirtualKeyModifiers.Control
        };
        searchAccelerator.Invoked += (sender, args) =>
        {
            // TODO: Focus search box
            System.Diagnostics.Debug.WriteLine("Ctrl+F - Search requested");
            args.Handled = true;
        };
        RootGrid.KeyboardAccelerators.Add(searchAccelerator);

        /*
         * KEYBOARD SHORTCUTS REFERENCE:
         *
         * Global Navigation:
         * - Alt+Left Arrow: Go back to previous page
         * - Alt+Home: Navigate to Guide Library (home)
         * - Ctrl+Comma: Open Settings
         * - F1: Help documentation (future)
         * - Ctrl+F: Search guides (future)
         *
         * Step Viewer (context-specific, implemented in StepViewerView):
         * - Left Arrow: Previous step
         * - Right Arrow: Next step
         * - Space: Toggle step completion
         * - Escape: Exit guide viewer
         *
         * List Navigation (context-specific):
         * - Up/Down Arrow: Navigate through items
         * - Enter: Open/activate selected item
         * - Delete: Delete selected item (admin only)
         *
         * General:
         * - Tab: Navigate forward through interactive elements
         * - Shift+Tab: Navigate backward through interactive elements
         * - Escape: Close dialog or cancel operation
         * - Enter: Confirm dialog or submit form
         */
    }
}
