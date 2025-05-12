using NaiveUI.NControls.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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


    }
}