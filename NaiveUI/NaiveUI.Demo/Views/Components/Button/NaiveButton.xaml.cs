
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

        

       
    }
}
