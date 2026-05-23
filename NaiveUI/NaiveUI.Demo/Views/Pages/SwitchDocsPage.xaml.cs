using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class SwitchDocsPage : UserControl
{
    private bool isEventDemoReady;
    private bool isAsyncLoadingDemoReady;
    private bool isSyncingAsyncLoadingDemo;

    public SwitchDocsPage()
    {
        InitializeComponent();
        DataContext = new SwitchDocsPageViewModel();
        Loaded += HandleLoaded;
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        isEventDemoReady = true;
        isAsyncLoadingDemoReady = true;
    }

    private void HandleOutlineItemInvoked(object? sender, NMenuItemInvokedEventArgs e)
    {
        if (e.Value is not string targetName)
        {
            return;
        }

        if (FindName(targetName) is FrameworkElement target)
        {
            target.BringIntoView();
        }
    }

    private void HandleEventSwitchValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (!isEventDemoReady || DataContext is not SwitchDocsPageViewModel viewModel)
        {
            return;
        }

        viewModel.RecordEventChange(e.OldValue, e.NewValue);
    }

    private async void HandleAsyncLoadingSwitchValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (!isAsyncLoadingDemoReady || isSyncingAsyncLoadingDemo || DataContext is not SwitchDocsPageViewModel viewModel)
        {
            return;
        }

        isSyncingAsyncLoadingDemo = true;

        try
        {
            viewModel.BeginAsyncLoadingChange(e.OldValue);
            await viewModel.CompleteAsyncLoadingChangeAsync(e.NewValue);
        }
        finally
        {
            isSyncingAsyncLoadingDemo = false;
        }
    }
}
