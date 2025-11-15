using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels;
using InstallVibe.Services;

namespace InstallVibe.Views;

public sealed partial class StepViewerView : Page
{
    public StepViewerViewModel ViewModel { get; }
    private readonly INavigationService _navigationService;

    public StepViewerView()
    {
        ViewModel = App.Services.GetRequiredService<StepViewerViewModel>();
        _navigationService = App.Services.GetRequiredService<INavigationService>();
        DataContext = ViewModel;
        InitializeComponent();

        // Initialize when page is loaded
        Loaded += async (s, e) =>
        {
            var parameters = _navigationService.GetNavigationParameters(typeof(StepViewerView));
            if (parameters != null)
            {
                ViewModel.Initialize(parameters);
            }
        };
    }
}
