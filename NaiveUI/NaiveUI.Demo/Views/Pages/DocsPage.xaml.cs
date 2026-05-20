using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Pages;

public partial class DocsPage : UserControl
{
    private DocsPageViewModel ViewModel => (DocsPageViewModel)DataContext;

    public DocsPage()
    {
        InitializeComponent();
        DataContext = new DocsPageViewModel();
    }

    private void HandleSidebarItemInvoked(object? sender, NNavMenuItemInvokedEventArgs e)
    {
        if (e.Item is ComponentSidebarItemViewModel item)
        {
            ViewModel.ShowComponent(item);
        }
    }
}
