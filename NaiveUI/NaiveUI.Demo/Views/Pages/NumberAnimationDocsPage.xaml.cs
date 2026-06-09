using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;
using NaiveUI.NControls.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace NaiveUI.Demo.Views.Pages;

public partial class NumberAnimationDocsPage : System.Windows.Controls.UserControl
{
    public NumberAnimationDocsPage()
    {
        InitializeComponent();
        DataContext = new NumberAnimationDocsPageViewModel();
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

    private void HandleReplayButtonClick(object sender, RoutedEventArgs e)
    {
        DynamicNumberAnimation.Restart();
    }

    private void HandleManualPlayClick(object sender, RoutedEventArgs e)
    {
        ManualNumberAnimation.Play();
    }

    private void HandleManualRestartClick(object sender, RoutedEventArgs e)
    {
        ManualNumberAnimation.Restart();
    }

    private void HandleFormatPlayClick(object sender, RoutedEventArgs e)
    {
        FormatNumberAnimation.Play();
    }

    private void HandleFormatRestartClick(object sender, RoutedEventArgs e)
    {
        FormatNumberAnimation.Restart();
    }
    private readonly INMessageApi message = NMessage.UseMessage();
    private void HandleNumberAnimationStarted(object sender, RoutedEventArgs e)
    {
        if (DataContext is NumberAnimationDocsPageViewModel viewModel)
        {
            message.Info("已开始");
            viewModel.EventText = "已开始";
        }
    }

    private void HandleNumberAnimationCompleted(object sender, RoutedEventArgs e)
    {
        if (DataContext is NumberAnimationDocsPageViewModel viewModel)
        {
            message.Info("已完成");
            viewModel.EventText = "已完成";
        }
    }
}
