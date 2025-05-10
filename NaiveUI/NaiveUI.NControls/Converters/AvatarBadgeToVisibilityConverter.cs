using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NaiveUI.NControls.Converters
{
    public class AvatarBadgeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string badge)
            {
                return string.IsNullOrEmpty(badge) ? Visibility.Collapsed : Visibility.Visible;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
