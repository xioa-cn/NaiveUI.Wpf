using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.Services;
using NaiveUI.Demo.ViewModels;

namespace NaiveUI.Demo.Views.Pages;

public partial class DividerDocsPage : UserControl
{
    private DividerDocsPageViewModel ViewModel => (DividerDocsPageViewModel)DataContext;

    public DividerDocsPage()
    {
        InitializeComponent();
        DataContext = new DividerDocsPageViewModel();
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
}
