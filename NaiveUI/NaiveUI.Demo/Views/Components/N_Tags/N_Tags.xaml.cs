using ElMessage.Wpf.Utils;
using NaiveUI.NControls.ControlsExample;
using System.Windows.Controls;

namespace NaiveUI.Demo.Views.Components.N_Tags;

public partial class N_Tags : UserControl {
    public N_Tags() {
        InitializeComponent();
    }

    private void N_Tag_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if(sender is N_Tag tag)
        {
            ElMessage.Wpf.Utils.ElMessage.Warning(tag.Text);
        }
    }
}