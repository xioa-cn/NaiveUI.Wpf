using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.Demo.Views.Pages;

public partial class TagDocsPage : UserControl
{
    public TagDocsPage()
    {
        InitializeComponent();
        DataContext = new TagDocsPageViewModel();
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

    private void HandleTagClick(object sender, RoutedEventArgs e)
    {
        NElMessage.Info("tag click");
    }

    private void HandleTagClose(object sender, RoutedEventArgs e)
    {
        NElMessage.Info("tag close");
    }
}
