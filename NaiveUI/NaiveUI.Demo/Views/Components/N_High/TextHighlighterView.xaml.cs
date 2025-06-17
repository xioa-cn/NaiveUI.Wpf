using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace NaiveUI.Demo.Views.Components.N_High;

public partial class TextHighlighterView : UserControl {
    public TextHighlighterView() {
        InitializeComponent();
    }
}
public class StringToWordsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            return text.Split(new[] { ',', '，', ';', '；', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
        return new string[0];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}