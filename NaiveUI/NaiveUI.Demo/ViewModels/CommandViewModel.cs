using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace NaiveUI.Demo.ViewModels;

public class CommandViewModel {

    public static CommandViewModel Instance { get; set; } = new CommandViewModel();
    
    public ICommand TestCommand { get; set; } = new RelayCommand<object>(Test);
    
    private static void Test(object? obj) {
        ElMessage.Wpf.Utils.ElMessage.Info("Command" + obj);
    }
}