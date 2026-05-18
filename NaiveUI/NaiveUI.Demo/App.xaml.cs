using NaiveUI.NControls.Themes;
using NaiveUI.NControls.Tools;
using System.Windows;

namespace NaiveUI.Demo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherHelper.Initialize();
        ThemeManager.ApplyTheme(ThemeMode.Light, this);
        base.OnStartup(e);
    }
}
