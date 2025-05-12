using NaiveUI.NControls.Tools;
using System.Windows;

namespace NaiveUI.NControls.Attach
{
    public class N_Tooltip
    {
        public static object GetContent(DependencyObject obj)
        {
            return (object)obj.GetValue(ContentProperty);
        }

        public static void SetContent(DependencyObject obj, object value)
        {
            obj.SetValue(ContentProperty, value);
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =         
            ElementBase.PropertyAttach<N_Tooltip, object?>(nameof(ContentProperty), default(object));
    }
}
