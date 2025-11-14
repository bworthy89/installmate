using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels;

namespace InstallVibe.Views;

public sealed partial class StepViewerView : Page
{
    public StepViewerViewModel ViewModel { get; }

    public StepViewerView(StepViewerViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();
    }
}
