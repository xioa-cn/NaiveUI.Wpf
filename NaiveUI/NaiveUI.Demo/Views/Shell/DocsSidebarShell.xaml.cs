using System;
using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Controls;

namespace NaiveUI.Demo.Views.Shell;

public partial class DocsSidebarShell : UserControl
{
    public static readonly DependencyProperty SidebarCategoriesProperty =
        DependencyProperty.Register(
            nameof(SidebarCategories),
            typeof(object),
            typeof(DocsSidebarShell),
            new PropertyMetadata(null));

    public DocsSidebarShell()
    {
        InitializeComponent();
    }

    public event EventHandler<NNavMenuItemInvokedEventArgs>? SidebarItemInvoked;

    public object? SidebarCategories
    {
        get => GetValue(SidebarCategoriesProperty);
        set => SetValue(SidebarCategoriesProperty, value);
    }

    private void HandleSidebarItemInvoked(object? sender, NNavMenuItemInvokedEventArgs e)
    {
        SidebarItemInvoked?.Invoke(this, e);
    }
}
