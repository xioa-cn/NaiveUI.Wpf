using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NIcon : Control
{
    static NIcon()
    {
        ElementBase.DefaultStyle<NIcon>(DefaultStyleKeyProperty);
    }

    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly DependencyProperty DataProperty =
        ElementBase.Property<NIcon, Geometry?>(nameof(DataProperty), null);

    public Brush IconBrush
    {
        get => (Brush)GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    public static readonly DependencyProperty IconBrushProperty =
        ElementBase.Property<NIcon, Brush>(nameof(IconBrushProperty), Brushes.Black);

    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public static readonly DependencyProperty StretchProperty =
        ElementBase.Property<NIcon, Stretch>(nameof(StretchProperty), Stretch.Uniform);
}
