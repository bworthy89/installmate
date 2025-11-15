using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels;

namespace InstallVibe.Views;

public sealed partial class GuideLibraryView : Page
{
    public GuideLibraryViewModel ViewModel { get; }

    public GuideLibraryView()
    {
        ViewModel = App.Services.GetRequiredService<GuideLibraryViewModel>();
        DataContext = ViewModel;
        InitializeComponent();

        // Load guides when page is fully loaded
        Loaded += async (s, e) => await ViewModel.LoadGuidesAsync();
    }

    private void GuideCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int guideId)
        {
            System.Diagnostics.Debug.WriteLine($"GuideCard_Click: Guide ID {guideId}");
            var guide = ViewModel.Guides.FirstOrDefault(g => g.Id == guideId);
            if (guide != null)
            {
                ViewModel.NavigateToGuideCommand.Execute(guide);
            }
        }
    }
}
