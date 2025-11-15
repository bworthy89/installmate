using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels;

namespace InstallVibe.Views;

public sealed partial class DiagnosticsView : Page
{
    public DiagnosticsViewModel ViewModel { get; }

    public DiagnosticsView()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<DiagnosticsViewModel>();
    }
}
