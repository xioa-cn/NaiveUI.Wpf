using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NaiveUI.NControls.Converters
{
    public class MarginDoubleValueConverter : IValueConverter
    {
        public int Margin { get; set; } = 0;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is double d)
            {
                return d - Margin * 2;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
