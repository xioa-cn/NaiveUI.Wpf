using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class InputDocsPage : UserControl
{
    public InputDocsPage()
    {
        InitializeComponent();
        DataContext = new InputDocsPageViewModel();
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

    private void HandleInputTextChanged(object sender, RoutedPropertyChangedEventArgs<string> e)
    {
        if (DataContext is InputDocsPageViewModel viewModel)
        {
            viewModel.RecordTextChanged(e.NewValue);
        }
    }

    private void HandleInputClear(object sender, RoutedEventArgs e)
    {
        if (DataContext is InputDocsPageViewModel viewModel)
        {
            viewModel.RecordClear();
        }
    }
}
