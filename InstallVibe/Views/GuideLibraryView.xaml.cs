using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels;

namespace InstallVibe.Views;

public sealed partial class GuideLibraryView : Page
{
    public GuideLibraryViewModel ViewModel { get; }

    public GuideLibraryView(GuideLibraryViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();

        // Load guides when page is fully loaded
        Loaded += async (s, e) => await ViewModel.LoadGuidesAsync();
    }
}
