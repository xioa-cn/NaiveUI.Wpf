using NaiveUI.Demo.Dialog;
using NaiveUI.NControls.ControlsExample;
using System.Windows.Controls;

namespace NaiveUI.Demo.Views.Components.N_Drawer;

public partial class DrawerView : UserControl
{
    public DrawerView()
    {
        N_Dialog.Register("DrawerView", this);
        InitializeComponent();
    }

    private async void OpenDrawerButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var result = await NaiveUI.NControls.ControlsExample.N_Drawer.ShowAsync("DrawerView", new DrawDialog(), true);
        await result.GetResultAsync();
    }


}