
using NaiveUI.NControls.ControlsExample;
using System.Windows.Controls;


namespace NaiveUI.Demo.Views.Components.N_Card
{
    /// <summary>
    /// NaiveCard.xaml 的交互逻辑
    /// </summary>
    public partial class NaiveCard : UserControl
    {
        public NaiveCard()
        {
            InitializeComponent();
        }

        private void N_SwitchClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if(sender is N_Switch nSwitch)
            {
                if (nSwitch.IsChecked == true)
                {
                    this.LoadingCardElement.SetValue(CardElement.SkeletonProperty,N_Skeleton.Loading);
                }
                else
                {
                    this.LoadingCardElement.SetValue(CardElement.SkeletonProperty, N_Skeleton.Normal);
                }
            }
        }
    }
}
