using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NLayoutHeader : ContentControl
{
    static NLayoutHeader()
    {
        ElementBase.DefaultStyle<NLayoutHeader>(DefaultStyleKeyProperty);
    }
}
