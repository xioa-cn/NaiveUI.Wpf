using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NaiveUI.Demo.Converter;

public class BoolToVisibility : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool val)
        {
            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        return DependencyProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}

public class EditViewShowAttach
{
    public static readonly DependencyProperty IsEditViewShowProperty =
        DependencyProperty.RegisterAttached(
            "IsEditViewShow",
            typeof(bool),
            typeof(EditViewShowAttach),
            new PropertyMetadata(false)
        );

    public static void SetIsEditViewShow(DependencyObject obj, bool value)
    {
        obj.SetValue(IsEditViewShowProperty, value);
    }


    public static bool GetIsEditViewShow(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsEditViewShowProperty);
    }
}