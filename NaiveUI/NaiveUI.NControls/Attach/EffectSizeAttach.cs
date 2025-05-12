using NaiveUI.NControls.Tools;
using System.Windows;

namespace NaiveUI.NControls.Attach
{
    public class EffectSizeAttach
    {
        public static double GetEffectSize(DependencyObject obj)
        {
            return (double)obj.GetValue(EffectSizeProperty);
        }

        public static void SetEffectSize(DependencyObject obj, double value)
        {
            obj.SetValue(EffectSizeProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EffectSizeProperty =
            ElementBase.PropertyAttach<EffectSizeAttach, double>(nameof(EffectSizeProperty), 0.8);


    }
}
