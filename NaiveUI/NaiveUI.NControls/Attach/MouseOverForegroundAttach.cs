using NaiveUI.NControls.Tools;
using System.Windows;
using System.Windows.Media;

namespace NaiveUI.NControls.Attach
{
    public class MouseOverForegroundAttach
    {
        public static Brush GetMouseOverForeground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(MouseOverForegroundProperty);
        }

        public static void SetMouseOverForeground(DependencyObject obj, Brush value)
        {
            obj.SetValue(MouseOverForegroundProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseOverForegroundProperty =
            ElementBase.PropertyAttach<MouseOverForegroundAttach, Brush?>(nameof(MouseOverForegroundProperty), default(Brush));

    }
}
