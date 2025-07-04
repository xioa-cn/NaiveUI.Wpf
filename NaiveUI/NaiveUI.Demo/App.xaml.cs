using NaiveUI.NControls.Tools;
using System.Configuration;
using System.Data;
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
        base.OnStartup(e);
    }
}