
using NaiveUI.NControls.Attach;
using System.Windows;
using System.Windows.Controls;


namespace NaiveUI.Demo.Views.Components.Button
{
    /// <summary>
    /// NaiveButton.xaml 的交互逻辑
    /// </summary>
    public partial class NaiveButton : UserControl
    {
        public NaiveButton()
        {
            InitializeComponent();
            this.Loaded += NaiveButton_Loaded;
        }

        private void NaiveButton_Loaded(object sender, RoutedEventArgs e)
        {
            Button.SetValue(ButtonLoadingAttach.LoadingProperty, true);
        }

        private void ClickEventExample(object sender, RoutedEventArgs e)
        {
            ElMessage.Wpf.Utils.ElMessage.Warning("Warning Message ~~~");
        }

        private void ClickEventExampleInfo(object sender, RoutedEventArgs e)
        {
            ElMessage.Wpf.Utils.ElMessage.Info("Warning Message ~~~");
        }

        private void ClickEventExampleSuccess(object sender, RoutedEventArgs e)
        {
            ElMessage.Wpf.Utils.ElMessage.Success("Warning Message ~~~");
        }

        private void ClickEventExampleError(object sender, RoutedEventArgs e)
        {
            ElMessage.Wpf.Utils.ElMessage.Error("Warning Message ~~~");
        }

        private void DefaultLoading(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
             btn.SetValue(ButtonLoadingAttach.LoadingProperty,true);


            Task.Run(async () =>
            {
                await Task.Delay(2000);
                Dispatcher.BeginInvoke(() =>
                {
                    btn.SetValue(ButtonLoadingAttach.LoadingProperty, false);
                });
            });

        }
    }
}
