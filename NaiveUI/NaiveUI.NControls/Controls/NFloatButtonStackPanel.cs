using System;
using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NFloatButtonStackPanel : Panel
{
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public static readonly DependencyProperty SpacingProperty =
        ElementBase.Property<NFloatButtonStackPanel, double>(nameof(SpacingProperty), 0d, OnSpacingPropertyChanged);

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = 0d;
        double height = 0d;
        var first = true;

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(availableSize.Width, double.PositiveInfinity));

            width = Math.Max(width, child.DesiredSize.Width);
            height += child.DesiredSize.Height;

            if (!first)
            {
                height += Spacing;
            }

            first = false;
        }

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var y = 0d;

        foreach (UIElement child in InternalChildren)
        {
            var childHeight = child.DesiredSize.Height;
            var childWidth = Math.Max(child.DesiredSize.Width, finalSize.Width);

            child.Arrange(new Rect(0d, y, childWidth, childHeight));
            y += childHeight + Spacing;
        }

        return finalSize;
    }

    private static void OnSpacingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NFloatButtonStackPanel panel)
        {
            return;
        }

        panel.InvalidateMeasure();
        panel.InvalidateArrange();
    }
}
