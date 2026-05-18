using System.Windows;
using System.Windows.Input;
using NaiveUI.Demo.ViewModels;

namespace NaiveUI.Demo.Views;

public partial class MainWindow : Window
{
    private DemoShellViewModel ViewModel => (DemoShellViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        StateChanged += (_, _) => ViewModel.SyncWindowState(WindowState);
        ViewModel.SyncWindowState(WindowState);
    }

    private void HandleHomeRequested(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowHomePage();
    }

    private void HandleComponentsRequested(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowComponentsPage();
    }

    private void HandleThemeToggleRequested(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleTheme();
    }

    private void HandleMinimizeRequested(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void HandleMaximizeRestoreRequested(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void HandleCloseRequested(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void HandleDragRequested(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            HandleMaximizeRestoreRequested(sender, new RoutedEventArgs());
            return;
        }

        if (WindowState == WindowState.Maximized)
        {
            return;
        }

        DragMove();
    }
}
