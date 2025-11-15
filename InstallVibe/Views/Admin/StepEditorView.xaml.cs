using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels;
using InstallVibe.Services;

namespace InstallVibe.Views.Admin;

public sealed partial class StepEditorView : Page
{
    public StepEditorViewModel ViewModel { get; }
    private readonly INavigationService _navigationService;

    public StepEditorView()
    {
        ViewModel = App.Services.GetRequiredService<StepEditorViewModel>();
        _navigationService = App.Services.GetRequiredService<INavigationService>();
        DataContext = ViewModel;
        InitializeComponent();

        // Initialize when page is loaded
        Loaded += (s, e) =>
        {
            var parameters = _navigationService.GetNavigationParameters(typeof(StepEditorView));
            if (parameters != null && parameters.ContainsKey("stepId"))
            {
                var stepId = (int)parameters["stepId"];
                ViewModel.Initialize(stepId);
            }
        };
    }

    private async void DeleteMediaButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not MediaItemViewModel media)
            return;

        var dialog = new ContentDialog
        {
            Title = "Delete Media",
            Content = $"Are you sure you want to delete '{media.FileName}'? This action cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteMediaCommand.ExecuteAsync(media);
        }
    }
}
