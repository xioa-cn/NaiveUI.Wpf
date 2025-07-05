using System.Windows;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Attach;

public class BorderRadiusAttach {
    public static CornerRadius GetBorderRadius(DependencyObject obj)
    {
        return (CornerRadius)obj.GetValue(BorderRadiusProperty);
    }

    public static void SetBorderRadius(DependencyObject obj, CornerRadius value)
    {
        obj.SetValue(BorderRadiusProperty, value);
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BorderRadiusProperty =    
        ElementBase.PropertyAttach<TriggerBackgroundAttach, CornerRadius?>(nameof(BorderRadiusProperty), default(CornerRadius));
}