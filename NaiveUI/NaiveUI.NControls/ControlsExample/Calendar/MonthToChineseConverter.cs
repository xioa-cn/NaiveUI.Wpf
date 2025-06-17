using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NaiveUI.NControls.ControlsExample;

public class MonthToChineseConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int iv)
        {
            return iv switch
            {
                1 => "一月",
                2 => "二月",
                3 => "三月",
                4 => "四月",
                5 => "五月",
                6 => "六月",
                7 => "七月",
                8 => "八月",
                9 => "九月",
                10 => "十月",
                11 => "十一月",
                12 => "十二月",
                _ => "未知月份"

            };
        }
        return DependencyProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}