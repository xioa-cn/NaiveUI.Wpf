using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NDividerOrientation
{
    Horizontal,
    Vertical
}

public enum NDividerTitlePlacement
{
    Start,
    Center,
    End
}

public class NDivider : Control
{
    static NDivider()
    {
        ElementBase.DefaultStyle<NDivider>(DefaultStyleKeyProperty);
    }

    public NDividerOrientation Orientation
    {
        get => (NDividerOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty =
        ElementBase.Property<NDivider, NDividerOrientation>(nameof(OrientationProperty), NDividerOrientation.Horizontal);

    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        ElementBase.Property<NDivider, object?>(nameof(TitleProperty), null);

    public NDividerTitlePlacement TitlePlacement
    {
        get => (NDividerTitlePlacement)GetValue(TitlePlacementProperty);
        set => SetValue(TitlePlacementProperty, value);
    }

    public static readonly DependencyProperty TitlePlacementProperty =
        ElementBase.Property<NDivider, NDividerTitlePlacement>(nameof(TitlePlacementProperty), NDividerTitlePlacement.Center);
}
