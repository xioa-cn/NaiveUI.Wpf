using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NaiveUI.NControls.ControlsExample;

public class IndexerConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] == null || values[1] == null)
            return null;
            
        var collection = values[0] as IList;
        if (collection == null)
            return null;
            
        int index = System.Convert.ToInt32(values[1]);
        if (index < 0 || index >= collection.Count)
            return null;
            
        return collection[index];
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return [DependencyProperty.UnsetValue];
    }
}