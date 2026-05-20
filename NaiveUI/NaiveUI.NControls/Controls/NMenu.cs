using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public sealed class NMenuItemInvokedEventArgs : EventArgs
{
    public NMenuItemInvokedEventArgs(object? item, object? value)
    {
        Item = item;
        Value = value;
    }

    public object? Item { get; }

    public object? Value { get; }
}

public class NMenu : ListBox
{
    static NMenu()
    {
        ElementBase.DefaultStyle<NMenu>(DefaultStyleKeyProperty);
    }

    public NMenu()
    {
        SetCurrentValue(SelectionModeProperty, SelectionMode.Single);
    }

    public event EventHandler<NMenuItemInvokedEventArgs>? ItemInvoked;

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);

        if (e.OriginalSource is not DependencyObject source)
        {
            return;
        }

        if (ItemsControl.ContainerFromElement(this, source) is not ListBoxItem container || !container.IsEnabled)
        {
            return;
        }

        var item = ItemContainerGenerator.ItemFromContainer(container);

        if (item == DependencyProperty.UnsetValue)
        {
            return;
        }

        if (!ReferenceEquals(SelectedItem, item))
        {
            SetCurrentValue(SelectedItemProperty, item);
        }

        RaiseItemInvoked(item);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if ((e.Key == Key.Enter || e.Key == Key.Space) && SelectedItem is not null)
        {
            RaiseItemInvoked(SelectedItem);
            e.Handled = true;
        }
    }

    private void RaiseItemInvoked(object? item)
    {
        var value = string.IsNullOrWhiteSpace(SelectedValuePath) ? item : SelectedValue;
        ItemInvoked?.Invoke(this, new NMenuItemInvokedEventArgs(item, value));
    }
}
