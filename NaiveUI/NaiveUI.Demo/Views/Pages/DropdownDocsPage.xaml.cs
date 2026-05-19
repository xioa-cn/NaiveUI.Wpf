using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.Services;
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

    private void ScrollToSection(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.Tag is not string targetName)
        {
            return;
        }

        if (FindName(targetName) is FrameworkElement target)
        {
            target.BringIntoView();
        }
    }

    private void HandleSidebarItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ComponentSidebarItemViewModel item })
        {
            ViewModel.SelectSidebarItem(item);
            DemoNavigationService.RequestComponent(item.Key);
        }
    }

    private void HandleDropdownSelect(object sender, NDropdownSelectEventArgs e)
    {
        ViewModel.RecordSelection(e.Key);
    }

    private void HandleOptionClick(object sender, NDropdownOptionClickEventArgs e)
    {
        NElMessage.Info(e.Key?.ToString() ?? "未提供键值");
        ViewModel.RecordOptionAction($"点击事件触发：{e.Key}");
    }

    private void HandleManualDropdownButtonClick(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleManualDropdown();
    }
}
