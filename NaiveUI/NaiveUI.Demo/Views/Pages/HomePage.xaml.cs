using System.Windows;
using System.Windows.Controls;
using NaiveUI.Demo.ViewModels;
using NaiveUI.NControls.Themes;

namespace NaiveUI.Demo.Views.Pages;

public partial class HomePage : UserControl
{
    public HomePage()
    {
        InitializeComponent();
    }

    private void HandleThemeClick(object sender, RoutedEventArgs e)
    {
        ThemeManager.Toggle(Application.Current);
    }

    private void HandleGetStartedClick(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this)?.DataContext is DemoShellViewModel viewModel)
        {
            viewModel.ShowComponentsPage();
        }
    }
}
