using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using InstallVibe.ViewModels;
using InstallVibe.Services;

namespace InstallVibe.Views;

public sealed partial class GuideViewerView : Page
{
    public GuideViewerViewModel ViewModel { get; }
    private readonly INavigationService _navigationService;

    public GuideViewerView(GuideViewerViewModel viewModel, INavigationService navigationService)
    {
        ViewModel = viewModel;
        _navigationService = navigationService;
        DataContext = ViewModel;
        InitializeComponent();

        // Initialize when page is loaded
        Loaded += async (s, e) =>
        {
            var parameters = _navigationService.GetNavigationParameters(typeof(GuideViewerView));
            if (parameters != null)
            {
                ViewModel.Initialize(parameters);
            }
        };

        // Handle keyboard shortcuts
        KeyDown += GuideViewerView_KeyDown;
    }

    private void GuideViewerView_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        // Left arrow: Previous step
        if (e.Key == Windows.System.VirtualKey.Left && ViewModel.CanGoPrevious)
        {
            ViewModel.PreviousStepCommand.Execute(null);
            e.Handled = true;
        }
        // Right arrow: Next step
        else if (e.Key == Windows.System.VirtualKey.Right && ViewModel.CanGoNext)
        {
            ViewModel.NextStepCommand.Execute(null);
            e.Handled = true;
        }
        // Space: Toggle complete
        else if (e.Key == Windows.System.VirtualKey.Space && ViewModel.CurrentStep != null)
        {
            ViewModel.MarkStepCompleteCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void StepButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int index)
        {
            if (index >= 0 && index < ViewModel.Steps.Count)
            {
                var step = ViewModel.Steps[index];
                ViewModel.SelectStepCommand.Execute(step);

                // Trigger transition animation
                if (StepTransitionStoryboard != null)
                {
                    StepTransitionStoryboard.Begin();
                }
            }
        }
    }

    private async void ResetProgressButton_Click(object sender, RoutedEventArgs e)
    {
        // Show confirmation dialog
        var dialog = new ContentDialog
        {
            Title = "Reset Progress",
            Content = "Are you sure you want to reset all progress for this guide? This action cannot be undone.",
            PrimaryButtonText = "Reset",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.ResetProgressCommand.ExecuteAsync(null);
        }
    }
}
