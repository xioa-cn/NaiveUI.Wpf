using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NLayout : ContentControl
{
    static NLayout()
    {
        ElementBase.DefaultStyle<NLayout>(DefaultStyleKeyProperty);
    }
}
