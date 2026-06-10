using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class MarqueeDocsPage : UserControl
{
    private MarqueeDocsPageViewModel ViewModel => (MarqueeDocsPageViewModel)DataContext;

    public MarqueeDocsPage()
    {
        InitializeComponent();
        DataContext = new MarqueeDocsPageViewModel();
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

    private void HandlePlayClick(object sender, RoutedEventArgs e)
    {
        ControlledMarquee.Play();
        ViewModel.EventText = "已播放";
    }

    private void HandlePauseClick(object sender, RoutedEventArgs e)
    {
        ControlledMarquee.Pause();
        ViewModel.EventText = "已暂停";
    }

    private void HandleResumeClick(object sender, RoutedEventArgs e)
    {
        ControlledMarquee.Resume();
        ViewModel.EventText = "继续播放";
    }

    private void HandleRestartClick(object sender, RoutedEventArgs e)
    {
        ControlledMarquee.Restart();
        ViewModel.EventText = "重新播放";
    }

    private void HandleStarted(object sender, RoutedEventArgs e)
    {
        ViewModel.EventText = "Started";
    }

    private void HandleStopped(object sender, RoutedEventArgs e)
    {
        ViewModel.EventText = "Stopped";
    }

    private void HandleCycleCompleted(object sender, RoutedEventArgs e)
    {
        ViewModel.EventText = "CycleCompleted";
    }
}
