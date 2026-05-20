using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using NaiveUI.Demo.Services;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class AvatarDocsPage : UserControl
{
    private static readonly Uri InvalidAvatarUri = new("https://naive-ui.invalid/avatar.png", UriKind.Absolute);
    private AvatarDocsPageViewModel ViewModel => (AvatarDocsPageViewModel)DataContext;

    public AvatarDocsPage()
    {
        InitializeComponent();
        DataContext = new AvatarDocsPageViewModel();
        InitializeBrokenAvatarSamples();
    }

    private void InitializeBrokenAvatarSamples()
    {
        BrokenAvatarWithFallback.Src = new BitmapImage(InvalidAvatarUri);
        BrokenAvatarWithTextFallback.Src = new BitmapImage(InvalidAvatarUri);
        BrokenAvatarWithPlaceholder.Src = new BitmapImage(InvalidAvatarUri);
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
}
