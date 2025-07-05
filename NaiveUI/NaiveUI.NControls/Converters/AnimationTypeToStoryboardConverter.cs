using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NaiveUI.NControls.Converters;

public class AnimationTypeToStoryboardConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string animationType)
        {
            switch (animationType.ToLower())
            {
                case "fade":
                    return Application.Current.Resources["FadeAnimation"];
                case "slidefromright":
                    return Application.Current.Resources["SlideFromRightAnimation"];
                case "slidefromleft":
                    return Application.Current.Resources["SlideFromLeftAnimation"];
                case "zoom":
                    return Application.Current.Resources["ZoomAnimation"];
                default:
                    return Application.Current.Resources["FadeAnimation"];
            }
        }
        return Application.Current.Resources["FadeAnimation"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return DependencyProperty.UnsetValue;
    }
}