using NaiveUI.NControls.ControlsExample;
using System.Windows;
using System.Windows.Controls;

namespace NaiveUI.Demo.Views.Components.N_Drawer;

public partial class DrawerView : UserControl {
    public DrawerView() {
        InitializeComponent();
    }

    private void OpenDrawerButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        drawer.IsOpen = !drawer.IsOpen;
    }
}