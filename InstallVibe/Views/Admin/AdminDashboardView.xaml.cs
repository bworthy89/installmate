using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels;

namespace InstallVibe.Views.Admin;

public sealed partial class AdminDashboardView : Page
{
    public AdminDashboardViewModel ViewModel { get; }

    public AdminDashboardView(AdminDashboardViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();

        // Load recent guides when page loads
        Loaded += async (s, e) =>
        {
            await ViewModel.LoadRecentGuidesAsync();
        };
    }

    private void EditGuideButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int guideId)
        {
            ViewModel.EditGuideCommand.Execute(guideId);
        }
    }
}
