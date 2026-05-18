using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NInput : TextBox
{
    static NInput()
    {
        ElementBase.DefaultStyle<NInput>(DefaultStyleKeyProperty);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        ElementBase.Property<NInput, string>(nameof(PlaceholderProperty), string.Empty);

    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    public static readonly DependencyProperty IsInvalidProperty =
        ElementBase.Property<NInput, bool>(nameof(IsInvalidProperty), false);
}
