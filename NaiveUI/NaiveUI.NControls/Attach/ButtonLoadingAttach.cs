using NaiveUI.NControls.ControlsExample.Loading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NaiveUI.NControls.Attach
{
    public class ButtonLoadingAttach
    {
        public static bool GetLoading(DependencyObject obj)
        {
            return (bool)obj.GetValue(LoadingProperty);
        }

        public static void SetLoading(DependencyObject obj, bool value)
        {
            obj.SetValue(LoadingProperty, value);
        }

        // Using a DependencyProperty as the backing store for Loading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.RegisterAttached("Loading", typeof(bool), typeof(ButtonLoadingAttach), new PropertyMetadata(false, LoadingMethod));

        private static void LoadingMethod(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue) return;

            if (d is Button btn)
            {
                if ((bool)e.NewValue)
                {
                    var b = GetLoadingBrush(btn);
                    var yContent = btn.Content;
                    if (yContent is Grid grid && grid.Name == "LoadingStackPanel")
                    {
                        var t = new LoadingAnimation { Name = "LoadingIcon" };
                        if (b is not null)
                        {
                            t.BrushElement = b;
                        }

                        grid.Children.Add(t);
                        Grid.SetColumn(t, 0);
                    }
                    else
                    {
                        grid = new Grid() { Name = "LoadingStackPanel" };
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        var t = new LoadingAnimation { Name = "LoadingIcon" };
                        if (b is not null)
                        {
                            t.BrushElement = b;
                        }
                        grid.Children.Add(t);
                        Grid.SetColumn(t, 0);
                        if (yContent is string str)
                        {
                            AccessText accessText = new AccessText() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                            accessText.Text = str;
                            grid.Children.Add(accessText);
                            Grid.SetColumn(accessText, 1);
                        }
                        else if(yContent is not null)
                        {
                            var element = yContent as UIElement;
                            if (element is null)
                            {
                               
                            }
                            else
                            {
                                grid.Children.Add(element);
                                Grid.SetColumn(element, 1);
                            }
                               

                        }
                        btn.Content = grid;
                    }
                    btn.Opacity = 0.5;

                }
                else
                {
                    var content = btn.Content;
                    var grid = content as Grid;

                    if (grid.Name == "LoadingStackPanel")
                    {
                        foreach (var co in grid.Children)
                        {
                            if (co is LoadingAnimation loading)
                            {
                                grid.Children.Remove(loading);
                                break;
                            }
                        }
                        btn.Opacity = 1;
                    }
                }

            }
        }





        public static Brush? GetLoadingBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(LoadingBrushProperty);
        }

        public static void SetLoadingBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(LoadingBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadingBrushProperty =
            DependencyProperty.RegisterAttached("LoadingBrush", typeof(Brush), typeof(ButtonLoadingAttach), new PropertyMetadata(null));


    }
}
