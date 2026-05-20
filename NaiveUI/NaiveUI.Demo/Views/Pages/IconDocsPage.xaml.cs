using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class IconDocsPage : UserControl
{
    private IconDocsPageViewModel ViewModel => (IconDocsPageViewModel)DataContext;

    public IconDocsPage()
    {
        InitializeComponent();
        DataContext = new IconDocsPageViewModel();
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
}
