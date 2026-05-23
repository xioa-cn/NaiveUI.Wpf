using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.Demo.Views.Pages;

public partial class MessageDocsPage : UserControl
{
    public MessageDocsPage()
    {
        InitializeComponent();
        DataContext = new MessageDocsPageViewModel();
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

    private void HandleScopedProviderMessageClick(object sender, RoutedEventArgs e)
    {
        LocalMessageProvider.UseMessage().Success("这条消息显示在局部 provider 中。");
    }
}
