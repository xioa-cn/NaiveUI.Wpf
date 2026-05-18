using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace NaiveUI.Demo.Views.Sections;

public partial class ApiDocTable : UserControl
{
    public ApiDocTable()
    {
        InitializeComponent();
    }

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ApiDocTable), new PropertyMetadata(null));
}
