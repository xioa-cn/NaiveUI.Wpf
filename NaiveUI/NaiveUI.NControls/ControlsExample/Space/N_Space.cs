using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.ControlsExample;

public class N_Space : StackPanel {
    static N_Space() {
        ElementBase.DefaultStyle<N_Space>(DefaultStyleKeyProperty);
    }
}