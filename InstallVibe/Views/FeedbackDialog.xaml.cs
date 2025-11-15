using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels;

namespace InstallVibe.Views;

public sealed partial class FeedbackDialog : ContentDialog
{
    public FeedbackViewModel ViewModel { get; }

    public FeedbackDialog(FeedbackViewModel viewModel)
    {
        this.InitializeComponent();
        ViewModel = viewModel;
    }
}
