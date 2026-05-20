using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class CardDocsPage : UserControl
{
    private CardDocsPageViewModel ViewModel => (CardDocsPageViewModel)DataContext;

    public CardDocsPage()
    {
        InitializeComponent();
        DataContext = new CardDocsPageViewModel();
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
