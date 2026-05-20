using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.Services;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class EllipsisDocsPage : UserControl
{
    private EllipsisDocsPageViewModel ViewModel => (EllipsisDocsPageViewModel)DataContext;

    public EllipsisDocsPage()
    {
        InitializeComponent();
        DataContext = new EllipsisDocsPageViewModel();
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

    private void HandleSidebarItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ComponentSidebarItemViewModel item })
        {
            ViewModel.SelectSidebarItem(item);
            DemoNavigationService.RequestComponent(item.Key);
        }
    }

    private void HandleToggleDynamicTextClick(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleDynamicText();
    }
}
