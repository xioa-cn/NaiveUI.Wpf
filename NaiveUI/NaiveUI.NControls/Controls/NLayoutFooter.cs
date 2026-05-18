using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NLayoutFooter : ContentControl
{
    static NLayoutFooter()
    {
        ElementBase.DefaultStyle<NLayoutFooter>(DefaultStyleKeyProperty);
    }
}
