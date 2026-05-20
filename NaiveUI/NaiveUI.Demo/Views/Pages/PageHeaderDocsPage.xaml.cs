using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.Demo.Views.Pages;

public partial class PageHeaderDocsPage : UserControl
{
    public PageHeaderDocsPage()
    {
        InitializeComponent();
        DataContext = new PageHeaderDocsPageViewModel();
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

    private void HandleExtraDropdownSelect(object sender, NDropdownSelectEventArgs e)
    {
        NElMessage.Info($"Select事件触发：{e.Key}");
    }

    private void BreadcrumbItemClick(object sender, RoutedEventArgs e)
    {
        NElMessage.Info($"Click事件触发：BreadcrumbItem");
    }
}
