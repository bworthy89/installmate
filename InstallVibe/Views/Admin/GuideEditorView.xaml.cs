using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels;
using InstallVibe.Services;

namespace InstallVibe.Views.Admin;

public sealed partial class GuideEditorView : Page
{
    public GuideEditorViewModel ViewModel { get; }
    private readonly INavigationService _navigationService;

    public GuideEditorView(GuideEditorViewModel viewModel, INavigationService navigationService)
    {
        ViewModel = viewModel;
        _navigationService = navigationService;
        DataContext = ViewModel;
        InitializeComponent();

        // Initialize when page is loaded
        Loaded += async (s, e) =>
        {
            var parameters = _navigationService.GetNavigationParameters(typeof(GuideEditorView));
            await ViewModel.InitializeAsync(parameters);
        };
    }

    private async void DeleteGuideButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Delete Guide",
            Content = "Are you sure you want to delete this guide? This action cannot be undone. All steps and media will be permanently deleted.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteGuideCommand.ExecuteAsync(null);
        }
    }

    private void MoveStepUpButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is StepEditorItem step)
        {
            ViewModel.MoveStepUpCommand.Execute(step);
        }
    }

    private void MoveStepDownButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is StepEditorItem step)
        {
            ViewModel.MoveStepDownCommand.Execute(step);
        }
    }

    private void EditStepButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is StepEditorItem step)
        {
            ViewModel.EditStepCommand.Execute(step);
        }
    }

    private async void DeleteStepButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not StepEditorItem step)
            return;

        var dialog = new ContentDialog
        {
            Title = "Delete Step",
            Content = $"Are you sure you want to delete '{step.Title}'? This action cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteStepCommand.ExecuteAsync(step);
        }
    }
}
