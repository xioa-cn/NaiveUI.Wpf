using NaiveUI.NControls.Themes;
using NaiveUI.NControls.Tools;
using System.Windows;
using ShowMeTheXAML;

namespace NaiveUI.Demo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        XamlDisplay.Init();
        DispatcherHelper.Initialize();
        ThemeManager.ApplyTheme(ThemeMode.Light, this);
        base.OnStartup(e);
    }
}
