using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.Demo.Views.Pages;

public partial class BreadcrumbDocsPage : UserControl
{
    public BreadcrumbDocsPage()
    {
        InitializeComponent();
        DataContext = new BreadcrumbDocsPageViewModel();
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

    private void HandleCustomDropdownSelect(object sender, NDropdownSelectEventArgs e)
    {
        NElMessage.Info($"Select事件触发：{e.Key}");
    }

    private void NBreadcrumbItemClick(object sender, RoutedEventArgs e)
    {
        NElMessage.Info($"Click事件触发");
    }
}
