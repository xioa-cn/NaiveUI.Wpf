using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class FloatButtonDocsPage : UserControl
{
    private FloatButtonDocsPageViewModel ViewModel => (FloatButtonDocsPageViewModel)DataContext;

    public FloatButtonDocsPage()
    {
        InitializeComponent();
        DataContext = new FloatButtonDocsPageViewModel();
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
