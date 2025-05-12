using NaiveUI.NControls.Tools;
using System.Windows;
using System.Windows.Shapes;

namespace NaiveUI.NControls.Attach
{
    public class IconAttach
    {
        public static Path? GetIcon(DependencyObject obj)
        {
            return (Path)obj.GetValue(IconProperty);
        }

        public static void SetIcon(DependencyObject obj, Path value)
        {
            obj.SetValue(IconProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =  
            ElementBase.PropertyAttach<IconAttach, Path?>(nameof(IconProperty), default(Path));


    }
}
