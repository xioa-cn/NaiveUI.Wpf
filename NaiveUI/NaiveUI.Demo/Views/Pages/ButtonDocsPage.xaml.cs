using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.Demo.Views.Pages;

public partial class ButtonDocsPage : UserControl
{
    private ButtonDocsPageViewModel ViewModel => (ButtonDocsPageViewModel)DataContext;

    public ButtonDocsPage()
    {
        InitializeComponent();
        DataContext = new ButtonDocsPageViewModel();
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

    private void HandleDemoButtonClick(object sender, RoutedEventArgs e)
    {
        NElMessage.Success("按钮 Click 事件已触发。");
    }
}
