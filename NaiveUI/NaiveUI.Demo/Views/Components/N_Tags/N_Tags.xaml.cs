using ElMessage.Wpf.Utils;
using NaiveUI.NControls.ControlsExample;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NaiveUI.Demo.Views.Components.N_Tags;

public partial class N_Tags : UserControl {
    public N_Tags() {
        InitializeComponent();
    }

    private void N_Tag_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if(sender is N_Tag tag)
        {
            ElMessage.Wpf.Utils.ElMessage.Warning(tag.Text);
        }
    }
}

public class BoolToColor : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is bool b)
        {
            return b ? Brushes.GreenYellow : Brushes.Red;
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}