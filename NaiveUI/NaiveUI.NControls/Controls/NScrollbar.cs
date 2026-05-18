using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NScrollbar : ScrollViewer
{
    static NScrollbar()
    {
        ElementBase.DefaultStyle<NScrollbar>(DefaultStyleKeyProperty);
    }
}
