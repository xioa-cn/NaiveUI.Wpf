using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NBreadcrumb : ItemsControl
{
    static NBreadcrumb()
    {
        ElementBase.DefaultStyle<NBreadcrumb>(DefaultStyleKeyProperty);
    }

    public NBreadcrumb()
    {
        Loaded += HandleLoaded;
    }

    public string Separator
    {
        get => (string)GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    public static readonly DependencyProperty SeparatorProperty =
        ElementBase.Property<NBreadcrumb, string>(nameof(SeparatorProperty), "/", OnSeparatorChanged);

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        RefreshItemsState();
    }

    private static void OnSeparatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NBreadcrumb breadcrumb)
        {
            breadcrumb.RefreshItemsState();
        }
    }

    internal void RefreshItemsState()
    {
        var lastIndex = Items.Count - 1;
        for (var i = 0; i < Items.Count; i++)
        {
            if (Items[i] is not NBreadcrumbItem item)
            {
                continue;
            }

            item.ParentBreadcrumb = this;
            item.UpdateResolvedState(i == lastIndex, Separator);
        }
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        RefreshItemsState();
    }
}
