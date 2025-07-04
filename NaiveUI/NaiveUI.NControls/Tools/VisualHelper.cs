

namespace NaiveUI.NControls.Tools
{
    public static class VisualHelper
    {
        public static T? GetChild<T>(System.Windows.DependencyObject? d) where T : System.Windows.DependencyObject
        {
            if (d == null)
            {
                return null;
            }

            if (d is T result)
            {
                return result;
            }

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(d); i++)
            {
                T? child = GetChild<T>(System.Windows.Media.VisualTreeHelper.GetChild(d, i));
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }
    }
}
