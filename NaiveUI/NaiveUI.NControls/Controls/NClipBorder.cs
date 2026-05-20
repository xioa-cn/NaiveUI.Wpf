using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NaiveUI.NControls.Controls;

public class NClipBorder : Border
{
    protected override Size ArrangeOverride(Size finalSize)
    {
        var arrangedSize = base.ArrangeOverride(finalSize);
        UpdateChildClip();
        return arrangedSize;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateChildClip();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == CornerRadiusProperty)
        {
            UpdateChildClip();
        }
    }

    private void UpdateChildClip()
    {
        if (Child is null)
        {
            return;
        }

        var childSize = Child.RenderSize;
        if (childSize.Width <= 0d || childSize.Height <= 0d)
        {
            Child.Clip = null;
            return;
        }

        Child.Clip = CreateClipGeometry(childSize, CornerRadius);
    }

    private static Geometry CreateClipGeometry(Size size, CornerRadius cornerRadius)
    {
        var rect = new Rect(0d, 0d, size.Width, size.Height);

        var topLeft = Math.Max(0d, cornerRadius.TopLeft);
        var topRight = Math.Max(0d, cornerRadius.TopRight);
        var bottomRight = Math.Max(0d, cornerRadius.BottomRight);
        var bottomLeft = Math.Max(0d, cornerRadius.BottomLeft);

        NormalizePair(ref topLeft, ref topRight, size.Width);
        NormalizePair(ref bottomLeft, ref bottomRight, size.Width);
        NormalizePair(ref topLeft, ref bottomLeft, size.Height);
        NormalizePair(ref topRight, ref bottomRight, size.Height);

        if (AreEqual(topLeft, topRight) &&
            AreEqual(topLeft, bottomRight) &&
            AreEqual(topLeft, bottomLeft))
        {
            return new RectangleGeometry(rect, topLeft, topLeft);
        }

        var geometry = new StreamGeometry();
        using var context = geometry.Open();

        context.BeginFigure(new Point(rect.Left + topLeft, rect.Top), true, true);
        context.LineTo(new Point(rect.Right - topRight, rect.Top), true, false);
        AddArc(context, new Point(rect.Right, rect.Top + topRight), topRight);
        context.LineTo(new Point(rect.Right, rect.Bottom - bottomRight), true, false);
        AddArc(context, new Point(rect.Right - bottomRight, rect.Bottom), bottomRight);
        context.LineTo(new Point(rect.Left + bottomLeft, rect.Bottom), true, false);
        AddArc(context, new Point(rect.Left, rect.Bottom - bottomLeft), bottomLeft);
        context.LineTo(new Point(rect.Left, rect.Top + topLeft), true, false);
        AddArc(context, new Point(rect.Left + topLeft, rect.Top), topLeft);

        geometry.Freeze();
        return geometry;
    }

    private static void NormalizePair(ref double first, ref double second, double limit)
    {
        var sum = first + second;
        if (sum <= limit || sum <= 0d)
        {
            return;
        }

        var scale = limit / sum;
        first *= scale;
        second *= scale;
    }

    private static void AddArc(StreamGeometryContext context, Point target, double radius)
    {
        if (radius <= 0d)
        {
            context.LineTo(target, true, false);
            return;
        }

        context.ArcTo(
            target,
            new Size(radius, radius),
            0d,
            false,
            SweepDirection.Clockwise,
            true,
            false);
    }

    private static bool AreEqual(double first, double second)
    {
        return Math.Abs(first - second) < 0.1d;
    }
}
