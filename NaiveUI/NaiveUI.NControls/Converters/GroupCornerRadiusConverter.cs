using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NaiveUI.NControls.Converters;

public enum CornerRadiusHor {
    Left,
    Right
}

public class GroupCornerRadiusConverter : IValueConverter {
    public CornerRadiusHor CornerRadiusHor { get; set; } = CornerRadiusHor.Left;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is double dv)
        {
            if (this.CornerRadiusHor == CornerRadiusHor.Left)
            {
                return new CornerRadius(dv / 2, 0, 0, dv / 2);
            }
            else if (this.CornerRadiusHor == CornerRadiusHor.Right)
            {
                return new CornerRadius(0, dv / 2, dv / 2, 0);
            }
        }


        return DependencyProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return DependencyProperty.UnsetValue;
    }
}