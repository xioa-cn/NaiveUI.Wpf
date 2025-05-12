using NaiveUI.NControls.Tools;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace NaiveUI.NControls.Attach
{
    public enum ButtonNType
    {
        Null,
        _a,
    }
    public class ButtonTypeAttach
    {
        public static ButtonNType GetButtonType(DependencyObject obj)
        {
            return (ButtonNType)obj.GetValue(ButtonTypeProperty);
        }

        public static void SetButtonType(DependencyObject obj, ButtonNType value)
        {
            obj.SetValue(ButtonTypeProperty, value);
        }

        // Using a DependencyProperty as the backing store for ButtonType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonTypeProperty =
            ElementBase.PropertyAttach<ButtonTypeAttach, ButtonNType>(nameof(ButtonTypeProperty), default(ButtonNType), PropertyBtnChangeType);

        private static void PropertyBtnChangeType(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button btn)
            {

                if (e.NewValue is ButtonNType._a)
                {
                    btn.Style = Application.Current.FindResource("LinkButton") as Style;

                    var norClick = (bool)btn.GetValue(StutNormalClickProperty);
                    if (!norClick)
                    {
                        btn.Click += LinkClick;
                    }


                }
            }
        }




        public static string? GetHref(DependencyObject obj)
        {
            return (string)obj.GetValue(HrefProperty);
        }

        public static void SetHref(DependencyObject obj, string value)
        {
            obj.SetValue(HrefProperty, value);
        }

        // Using a DependencyProperty as the backing store for Href.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HrefProperty =
            ElementBase.PropertyAttach<ButtonTypeAttach, string?>(nameof(HrefProperty), default(string));
        public static bool GetStutNormalClick(DependencyObject obj)
        {
            return (bool)obj.GetValue(StutNormalClickProperty);
        }

        public static void SetStutNormalClick(DependencyObject obj, bool value)
        {
            obj.SetValue(StutNormalClickProperty, value);
        }

        // Using a DependencyProperty as the backing store for StutNormalClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StutNormalClickProperty =
            ElementBase.PropertyAttach<ButtonTypeAttach, bool?>(nameof(StutNormalClickProperty), default(bool), ShutChanged);

        private static void ShutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button btn)
            {
                if ((bool)e.NewValue)
                {
                    try
                    {
                        btn.Click -= LinkClick;
                    }
                    catch (Exception)
                    {


                    }

                }
                else
                {
                    try
                    {
                        btn.Click += LinkClick;
                    }
                    catch (Exception)
                    {


                    }
                }
            }
        }

        private static void LinkClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button cbtn)
            {
                var url = cbtn.GetValue(HrefProperty);
                WebBrowser wBrowser = new WebBrowser();

                Window win = new Window();
                win.Content = wBrowser;
                wBrowser.Navigate(url.ToString());
                win.Title = url.ToString();
                FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fiComWebBrowser == null) return;

                object objComWebBrowser = fiComWebBrowser.GetValue(wBrowser);
                if (objComWebBrowser == null) return;

                objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { true });

                win.ShowDialog();

            }
        }
    }
}
