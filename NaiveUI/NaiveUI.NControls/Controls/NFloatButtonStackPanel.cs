using System;
using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NFloatButtonStackPanel : Panel
{
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty =
        ElementBase.Property<NFloatButtonStackPanel, Orientation>(nameof(OrientationProperty), Orientation.Vertical, OnLayoutPropertyChanged);

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public static readonly DependencyProperty SpacingProperty =
        ElementBase.Property<NFloatButtonStackPanel, double>(nameof(SpacingProperty), 0d, OnSpacingPropertyChanged);

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Orientation == Orientation.Horizontal)
        {
            double width = 0d;
            double height = 0d;
            var firstHorizontal = true;

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(double.PositiveInfinity, availableSize.Height));

                width += child.DesiredSize.Width;
                height = Math.Max(height, child.DesiredSize.Height);

                if (!firstHorizontal)
                {
                    width += Spacing;
                }

                firstHorizontal = false;
            }

            return new Size(width, height);
        }

        double measuredWidth = 0d;
        double measuredHeight = 0d;
        var first = true;

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(availableSize.Width, double.PositiveInfinity));

            measuredWidth = Math.Max(measuredWidth, child.DesiredSize.Width);
            measuredHeight += child.DesiredSize.Height;

            if (!first)
            {
                measuredHeight += Spacing;
            }

            first = false;
        }

        return new Size(measuredWidth, measuredHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Orientation == Orientation.Horizontal)
        {
            var x = 0d;

            foreach (UIElement child in InternalChildren)
            {
                var childWidth = child.DesiredSize.Width;
                var childHeight = Math.Max(child.DesiredSize.Height, finalSize.Height);

                child.Arrange(new Rect(x, 0d, childWidth, childHeight));
                x += childWidth + Spacing;
            }

            return finalSize;
        }

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
        InvalidateLayout(d);
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        InvalidateLayout(d);
    }

    private static void InvalidateLayout(DependencyObject d)
    {
        if (d is not NFloatButtonStackPanel panel)
        {
            return;
        }

        panel.InvalidateMeasure();
        panel.InvalidateArrange();
    }
}
