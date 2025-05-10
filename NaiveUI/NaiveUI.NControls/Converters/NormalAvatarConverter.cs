using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NaiveUI.NControls.Converters
{
    public class NormalAvatarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is null)
            {
                var data = (StreamGeometry)Application.Current.FindResource("Avatar_Normal");
                return new Path()
                {
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Data = data,
                    Fill = (Brush)Application.Current.FindResource("Main.Brush"),
                };
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
