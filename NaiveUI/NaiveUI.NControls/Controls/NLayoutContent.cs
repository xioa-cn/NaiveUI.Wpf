using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NLayoutContent : ContentControl
{
    static NLayoutContent()
    {
        ElementBase.DefaultStyle<NLayoutContent>(DefaultStyleKeyProperty);
    }
}
