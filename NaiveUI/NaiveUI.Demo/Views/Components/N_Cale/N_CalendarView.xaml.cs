using NaiveUI.NControls.ControlsExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NaiveUI.Demo.Views.Components.N_Cale
{
    /// <summary>
    /// N_CalendarView.xaml 的交互逻辑
    /// </summary>
    public partial class N_CalendarView : UserControl
    {
        public N_CalendarView()
        {
            InitializeComponent();
        }

        private void N_Calendar_Click(object sender, RoutedEventArgs e)
        {
            if(sender is N_Calendar calendar)
            {
                MessageBox.Show($"Event:当前选择日期：{calendar.SelectedDate.ToString("yyyy-MM-dd") ?? "未选择"}");
            }
        }
    }
}
