using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;
using NaiveUI.NControls.Tools;
using System.Windows;
using System.Windows.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class ColorPickerDocsPage : UserControl
{
    public ColorPickerDocsPage()
    {
        InitializeComponent();
        DataContext = new ColorPickerDocsPageViewModel();
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

    private void HandleValueChanged(object sender, NColorPickerValueChangedEventArgs e)
    {
        if (DataContext is ColorPickerDocsPageViewModel viewModel)
        {
            viewModel.RecordValueChanged(e.OldValue, e.NewValue);
        }
    }

    private void HandleComplete(object sender, NColorPickerCompleteEventArgs e)
    {
        if (DataContext is ColorPickerDocsPageViewModel viewModel)
        {
            viewModel.RecordComplete(e.Value);
        }
    }

    private void HandleDropDownOpenChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
    {
        if (DataContext is ColorPickerDocsPageViewModel viewModel)
        {
            viewModel.RecordOpenChanged(e.OldValue, e.NewValue);
        }
    }

    private void NColorPicker_ConfirmButtonClick(object sender, RoutedEventArgs e)
    {
        NMessage.UseMessage().Success("Click 点击确认按钮");
    }
}
