using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public sealed class NNavMenuItemInvokedEventArgs : EventArgs
{
    public NNavMenuItemInvokedEventArgs(object? item, object? value)
    {
        Item = item;
        Value = value;
    }

    public object? Item { get; }

    public object? Value { get; }
}

public class NNavMenu : ItemsControl
{
    public static readonly DependencyProperty ValueMemberPathProperty =
        DependencyProperty.Register(
            nameof(ValueMemberPath),
            typeof(string),
            typeof(NNavMenu),
            new PropertyMetadata("Key"));

    static NNavMenu()
    {
        ElementBase.DefaultStyle<NNavMenu>(DefaultStyleKeyProperty);
    }

    public NNavMenu()
    {
        AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(HandleItemButtonClick));
    }

    public event EventHandler<NNavMenuItemInvokedEventArgs>? ItemInvoked;

    public string ValueMemberPath
    {
        get => (string)GetValue(ValueMemberPathProperty);
        set => SetValue(ValueMemberPathProperty, value);
    }

    private void HandleItemButtonClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source)
        {
            return;
        }

        var button = FindAncestorOrSelf<Button>(source);
        if (button?.DataContext is null)
        {
            return;
        }

        RaiseItemInvoked(button.DataContext);
        e.Handled = true;
    }

    private void RaiseItemInvoked(object item)
    {
        var value = string.IsNullOrWhiteSpace(ValueMemberPath)
            ? item
            : GetPropertyValue(item, ValueMemberPath);

        ItemInvoked?.Invoke(this, new NNavMenuItemInvokedEventArgs(item, value));
    }

    private static object? GetPropertyValue(object source, string propertyName)
    {
        var property = source.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);

        return property?.GetValue(source);
    }

    private static T? FindAncestorOrSelf<T>(DependencyObject? source)
        where T : DependencyObject
    {
        while (source is not null)
        {
            if (source is T target)
            {
                return target;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return null;
    }
}
