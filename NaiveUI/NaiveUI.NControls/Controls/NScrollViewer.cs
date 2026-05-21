using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NScrollViewer : ScrollViewer
{
    static NScrollViewer()
    {
        ElementBase.DefaultStyle<NScrollViewer>(DefaultStyleKeyProperty);
    }
}
