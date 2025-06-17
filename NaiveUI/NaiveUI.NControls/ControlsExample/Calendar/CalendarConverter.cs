using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NaiveUI.NControls.ControlsExample;

public class CalendarConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
        {
            var result = DateHelper.GenerateMonthInfo(dt);
            while (result.Days[0].Date.DayOfWeek != DayOfWeek.Monday)
            {
                var add = result.Days[0].Date.AddDays(-1);
                result.Days.Insert(0, new DayInfo() { Date = add });

            }
            while (result.Days[result.Days.Count-1].Date.DayOfWeek != DayOfWeek.Sunday)
            {
                var add = result.Days[result.Days.Count - 1].Date.AddDays(1);
                result.Days.Add(new DayInfo() { Date = add });
            }
            return result.Days;
        }
        return DependencyProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}