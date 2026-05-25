using System.Windows;

namespace NaiveUI.NControls.Controls;

public class NSelectOption : DependencyObject
{
    public object? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(object), typeof(NSelectOption), new PropertyMetadata(null));

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(object), typeof(NSelectOption), new PropertyMetadata(null));

    public bool Disabled
    {
        get => (bool)GetValue(DisabledProperty);
        set => SetValue(DisabledProperty, value);
    }

    public static readonly DependencyProperty DisabledProperty =
        DependencyProperty.Register(nameof(Disabled), typeof(bool), typeof(NSelectOption), new PropertyMetadata(false));

    public bool Show
    {
        get => (bool)GetValue(ShowProperty);
        set => SetValue(ShowProperty, value);
    }

    public static readonly DependencyProperty ShowProperty =
        DependencyProperty.Register(nameof(Show), typeof(bool), typeof(NSelectOption), new PropertyMetadata(true));

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(NSelectOption), new PropertyMetadata(null));

    public object? Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public static readonly DependencyProperty SuffixProperty =
        DependencyProperty.Register(nameof(Suffix), typeof(object), typeof(NSelectOption), new PropertyMetadata(null));

    public object? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(object), typeof(NSelectOption), new PropertyMetadata(null));
}

public sealed class NSelectSelectionChangedEventArgs : RoutedEventArgs
{
    public NSelectSelectionChangedEventArgs(RoutedEvent routedEvent, object source, object? oldValue, object? newValue)
        : base(routedEvent, source)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public object? OldValue { get; }

    public object? NewValue { get; }
}

public sealed class NSelectTagItem
{
    public NSelectTagItem(object? label, object? value)
    {
        Label = label;
        Value = value;
    }

    public object? Label { get; }

    public object? Value { get; }
}
