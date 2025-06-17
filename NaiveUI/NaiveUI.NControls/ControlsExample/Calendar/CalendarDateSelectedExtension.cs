using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace NaiveUI.NControls.ControlsExample;

[MarkupExtensionReturnType(typeof(MouseButtonEventHandler))]
public class CalendarDateSelectedExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new RoutedEventHandler(OnDateSelected);
    }
        
    private void OnDateSelected(object sender, RoutedEventArgs e)
    {
        if (sender is Border border && border.DataContext is DayInfo dayInfo)
        {
            // 查找最近的N_Calendar控件
            DependencyObject parent = VisualTreeHelper.GetParent(border);
            while (parent != null && !(parent is N_Calendar))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
                
            if (parent is N_Calendar calendar)
            {
                calendar.SelectChangedEvent(dayInfo);
                e.Handled = true;
            }
        }
    }
}