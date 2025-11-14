using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels;

namespace InstallVibe.Views;

public sealed partial class GuideDetailView : Page
{
    public GuideDetailViewModel ViewModel { get; }

    public GuideDetailView(GuideDetailViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();
    }
}
