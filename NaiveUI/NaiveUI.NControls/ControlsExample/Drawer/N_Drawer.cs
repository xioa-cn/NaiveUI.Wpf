using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using NaiveUI.NControls.Tools;


namespace NaiveUI.NControls.ControlsExample;

public class N_Drawer : ContentControl {
    public static readonly DependencyProperty PlacementProperty =
        ElementBase.Property<N_Drawer, DrawerPlacement>(nameof(PlacementProperty), DrawerPlacement.Left);

    public DrawerPlacement Placement {
        get => (DrawerPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public static readonly DependencyProperty TemplateViewProperty =
        ElementBase.Property<N_Drawer, object>(nameof(TemplateViewProperty), null);

    public object TemplateView {
        get => GetValue(TemplateViewProperty);
        set => SetValue(TemplateViewProperty, value);
    }

    public static readonly DependencyProperty IsOpenProperty =
        ElementBase.Property<N_Drawer, bool>(nameof(IsOpenProperty), false, OpenOnPage);

    private static void OpenOnPage(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is not N_Drawer nDrawer) return;
        if (nDrawer.IsOpen)
        {
            nDrawer.Show();
        }
        else
        {
            nDrawer.Hide();
        }
    }
    public static readonly DependencyProperty IsOverlayClickableProperty =
       ElementBase.Property<N_Drawer, bool>(nameof(IsOverlayClickableProperty), true);
    public bool IsOverlayClickable
    {
        get => (bool)GetValue(IsOverlayClickableProperty);
        set => SetValue(IsOverlayClickableProperty, value);
    }

    private OverlayAdorner _overlayAdorner; 

    private void Show()
    {
        var window = Window.GetWindow(this);
        // 获取当前控件的 AdornerLayer（确保控件已加载到视觉树）
        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
        if (adornerLayer == null)
        {
            System.Diagnostics.Debug.WriteLine("AdornerLayer 获取失败，控件未加载到视觉树");
            return;
        }

        // 避免重复添加 Adorner
        if (_overlayAdorner == null)
        {
            _overlayAdorner = new OverlayAdorner(window, this);           
            adornerLayer.Add(_overlayAdorner);
        }

        // 可选：显式设置 Adorner 可见性（若之前隐藏过）
        _overlayAdorner.Visibility = Visibility.Visible;
    }

    private void Hide() {
    }

    public bool IsOpen {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
}

public enum DrawerPlacement {
    Left,
    Right,
    Top,
    Bottom
}