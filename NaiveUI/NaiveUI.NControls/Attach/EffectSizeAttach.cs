using System.Windows;

namespace NaiveUI.NControls.Attach
{
    public  class EffectSizeAttach
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
            DependencyProperty.RegisterAttached("EffectSize", typeof(double), typeof(EffectSizeAttach), new PropertyMetadata(0.8));


    }
}
