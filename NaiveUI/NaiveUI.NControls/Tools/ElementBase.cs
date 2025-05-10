using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace NaiveUI.NControls.Tools
{
    public class ElementBase
    {
        public static DependencyProperty Property<TThisType, TPropertyType>(string name, TPropertyType defaultValue)
        {
            return DependencyProperty.Register(name.Replace("Property", ""), typeof(TPropertyType), typeof(TThisType),
                new PropertyMetadata(defaultValue));
        }


        public static DependencyProperty Property<TThisType, TPropertyType>(string name)
        {
            return DependencyProperty.Register(name.Replace("Property", ""), typeof(TPropertyType), typeof(TThisType));
        }


        public static RoutedEvent RoutedEvent<TThisType, TPropertyType>(string name)
        {
            return EventManager.RegisterRoutedEvent(name.Replace("Event", ""), RoutingStrategy.Bubble,
                typeof(TPropertyType),
                typeof(TThisType));
        }


        public static void DefaultStyle<TThisType>(DependencyProperty dp)
        {
            dp.OverrideMetadata(typeof(TThisType), new FrameworkPropertyMetadata(typeof(TThisType)));
        }


        public static RoutedUICommand Command<THisType>(string name)
        {
            return new RoutedUICommand(name, name, typeof(THisType));
        }


        public static string GoToState(FrameworkElement element, string state)
        {
            VisualStateManager.GoToState(element, state, false);
            return state;
        }
    }
}
