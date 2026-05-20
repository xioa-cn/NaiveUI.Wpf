using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.Demo.Views.Pages;

public partial class DropdownDocsPage : UserControl
{
    private DropdownDocsPageViewModel ViewModel => (DropdownDocsPageViewModel)DataContext;

    public DropdownDocsPage()
    {
        InitializeComponent();
        DataContext = new DropdownDocsPageViewModel();
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

    private void HandleDropdownSelect(object sender, NDropdownSelectEventArgs e)
    {
        var message = $"Select事件触发：{e.Key}";
        NElMessage.Info(message);
        ViewModel.RecordSelection(e.Key);
    }

    private void HandleOptionClick(object sender, NDropdownOptionClickEventArgs e)
    {
        var message = $"Click事件触发：{e.Key}";
        NElMessage.Info(message);
        ViewModel.RecordOptionAction(message);
    }

    private void HandleManualDropdownButtonClick(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleManualDropdown();
    }
}
