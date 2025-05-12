using NaiveUI.NControls.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NaiveUI.NControls.Attach;

public class GradientTextAttach
{
    public static double GetGradientTextSize(DependencyObject obj)
    {
        return (double)obj.GetValue(GradientTextFontSizeProperty);
    }

    public static void SetGradientTextFontSize(DependencyObject obj, double value)
    {
        obj.SetValue(GradientTextFontSizeProperty, value);
    }

    // Using a DependencyProperty as the backing store for GradientTextFontSize.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty GradientTextFontSizeProperty =
        ElementBase.PropertyAttach<GradientTextAttach, double>(nameof(GradientTextFontSizeProperty), 10, FontSizeChangedCallBack);

    private static void FontSizeChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        switch (d)
        {
            case TextBlock textBlock:
                textBlock.FontSize = (double)e.NewValue;
                break;
            case TextBox textBox:
                textBox.FontSize = (double)e.NewValue;
                break;
            case TextElement textElement:
                textElement.FontSize = (double)e.NewValue;
                break;
            case Label label:
                label.FontSize = (double)e.NewValue;
                break;
            case AccessText access:
                access.FontSize = (double)e.NewValue;
                break;
            default:
                break;
        }

        var brsuh = d.GetValue(GradientTextAttach.GradientTextBrushProperty);
        if(brsuh is null)
        {
            SetBrush(d, DefaultBrush());
        }
    }



    public static Brush? GetGradientTextBrush(DependencyObject obj)
    {
        return (Brush?)obj.GetValue(GradientTextBrushProperty);
    }

    public static void SetGradientTextBrush(DependencyObject obj, Brush value)
    {
        obj.SetValue(GradientTextBrushProperty, value);
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty GradientTextBrushProperty =
       ElementBase.PropertyAttach<GradientTextAttach, Brush?>(nameof(GradientTextBrushProperty), null, GradientTextBrushChangedCallBack);

    private static void GradientTextBrushChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not Brush newBrush) return;

        SetBrush(d, newBrush);
    }

    private static void SetBrush(DependencyObject d, Brush newBrush)
    {
        if (newBrush is null) newBrush = DefaultBrush();
        switch (d)
        {
            case TextBlock textBlock:
                textBlock.Foreground = newBrush;
                break;
            case TextBox textBox:
                textBox.Foreground = newBrush;
                break;
            case TextElement textElement:
                textElement.Foreground = newBrush;
                break;
            case Label label:
                label.Foreground = newBrush;
                break;
            case AccessText access:
                access.Foreground = newBrush;
                break;
            default:
                throw new ArgumentException(nameof(d));
        }
    }

    private static Brush DefaultBrush()
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Colors.Blue, 0.0),
                new GradientStop(Colors.Red, 1.0)
            }
        };

        return brush;
    }
}