using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NaiveUI.NControls.Converters
{
    public class ThumbMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double railWidth)
            {
                return new Thickness(railWidth - 20, 0, 0, 0);
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}