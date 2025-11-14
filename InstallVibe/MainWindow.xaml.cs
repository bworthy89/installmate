using Microsoft.UI.Xaml;
using InstallVibe.Services;

namespace InstallVibe;

public sealed partial class MainWindow : Window
{
    public MainWindow(INavigationService navigationService)
    {
        InitializeComponent();

        // Set the navigation frame
        navigationService.Frame = ContentFrame;

        // Set window size
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));

        // Center the window
        CenterWindow();
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
}
