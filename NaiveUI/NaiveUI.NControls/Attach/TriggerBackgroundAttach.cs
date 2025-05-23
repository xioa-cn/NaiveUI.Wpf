﻿using NaiveUI.NControls.Tools;
using System.Windows;
using System.Windows.Media;

namespace NaiveUI.NControls.Attach
{
    public class TriggerBackgroundAttach
    {


        public static Brush GetTriggerBackground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(TriggerBackgroundProperty);
        }

        public static void SetTriggerBackground(DependencyObject obj, Brush value)
        {
            obj.SetValue(TriggerBackgroundProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TriggerBackgroundProperty =    
            ElementBase.PropertyAttach<TriggerBackgroundAttach, Brush?>(nameof(TriggerBackgroundProperty), default(Brush));


    }
}
