

using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Input;

namespace NaiveUI.Demo.ViewModels
{
    public class CalendarViewModel
    {
        public static CalendarViewModel Instance { get; } = new CalendarViewModel();
        public ICommand CalendarCommand { get; set; }

        public CalendarViewModel()
        {
            this.CalendarCommand = new RelayCommand<DateTime>(OnCalendarCommandExecuted);
        }

        private void OnCalendarCommandExecuted(DateTime time)
        {
            MessageBox.Show($"Command:当前选择日期：{time.ToString("yyyy-MM-dd") ?? "未选择"}", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
