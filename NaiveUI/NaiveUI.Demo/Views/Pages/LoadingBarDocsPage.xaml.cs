using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class LoadingBarDocsPage : UserControl
{
    public LoadingBarDocsPage()
    {
        InitializeComponent();
        DataContext = new LoadingBarDocsPageViewModel();
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

    private void HandleScopedLoadingBarStartClick(object sender, RoutedEventArgs e)
    {
        var loadingBar = LocalLoadingBarProvider.UseLoadingBar();
        loadingBar.Start();
        loadingBar.Update(0.42d);
    }

    private void HandleScopedLoadingBarFinishClick(object sender, RoutedEventArgs e)
    {
        LocalLoadingBarProvider.UseLoadingBar().Finish();
    }

    private void HandleScopedLoadingBarErrorClick(object sender, RoutedEventArgs e)
    {
        LocalLoadingBarProvider.UseLoadingBar().Error();
    }

    private void HandleThickLoadingBarStartClick(object sender, RoutedEventArgs e)
    {
        var loadingBar = ThickLoadingBarProvider.UseLoadingBar();
        loadingBar.Start();
        loadingBar.Update(0.42d);
    }

    private void HandleThickLoadingBarFinishClick(object sender, RoutedEventArgs e)
    {
        ThickLoadingBarProvider.UseLoadingBar().Finish();
    }
}
