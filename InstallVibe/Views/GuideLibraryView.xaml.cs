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

        // Load guides when view is created
        _ = ViewModel.LoadGuidesAsync();
    }
}
