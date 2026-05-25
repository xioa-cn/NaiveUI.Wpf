using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class SelectDocsPage : UserControl
{
    public SelectDocsPage()
    {
        InitializeComponent();
        DataContext = new SelectDocsPageViewModel();
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

    private void HandleSelectionChanged(object sender, NSelectSelectionChangedEventArgs e)
    {
        if (DataContext is SelectDocsPageViewModel viewModel)
        {
            viewModel.RecordSelection(e.OldValue, e.NewValue);
        }
    }

    private void HandleHeaderActionClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is SelectDocsPageViewModel viewModel)
        {
            viewModel.RecordHeaderAction();
        }
    }

    private void HandleFocusClick(object sender, RoutedEventArgs e)
    {
        DemoFocusSelect.Focus();
        if (DataContext is SelectDocsPageViewModel viewModel)
        {
            viewModel.RecordFocusAction("Focus()");
        }
    }

    private void HandleBlurClick(object sender, RoutedEventArgs e)
    {
        DemoFocusSelect.Blur();
        if (DataContext is SelectDocsPageViewModel viewModel)
        {
            viewModel.RecordFocusAction("Blur()");
        }
    }
}
