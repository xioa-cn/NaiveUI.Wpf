using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class CarouselDocsPage : UserControl
{
    private CarouselDocsPageViewModel ViewModel => (CarouselDocsPageViewModel)DataContext;

    public CarouselDocsPage()
    {
        InitializeComponent();
        DataContext = new CarouselDocsPageViewModel();
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
