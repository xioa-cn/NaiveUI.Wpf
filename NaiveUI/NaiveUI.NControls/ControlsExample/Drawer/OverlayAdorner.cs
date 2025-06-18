using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NaiveUI.NControls.ControlsExample;

// 添加到 N_Drawer 类外部或内部（根据项目结构）
// 实现OverlayAdorner类
public class OverlayAdorner : Adorner
{
    private readonly N_Drawer _drawer;
    private readonly Window _window;
    private readonly Grid _rootGrid;
    private readonly Rectangle _overlay;
    private readonly ContentControl _drawerContent;
    private bool _isDrawerOpen;

    public OverlayAdorner(Window window, N_Drawer drawer) : base(window)
    {
        _window = window;
        _drawer = drawer;
        _isDrawerOpen = false;

        // 创建根网格
        _rootGrid = new Grid
        {
            Width = 200,
            Height = 200,
            Background = Brushes.Red
        };

        // 创建遮罩层
        _overlay = new Rectangle
        {
            Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
            Width = window.ActualWidth,
            Height = window.ActualHeight,
            IsHitTestVisible = true
        };
        _overlay.MouseLeftButtonDown += Overlay_MouseLeftButtonDown;

        // 创建抽屉内容
        _drawerContent = new ContentControl
        {
            Content = drawer.TemplateView,
            Width = drawer.Width > 0 ? drawer.Width : 300,
            Visibility = Visibility.Collapsed
        };

        // 添加子元素到根网格
        _rootGrid.Children.Add(_overlay);
        _rootGrid.Children.Add(_drawerContent);

        // 添加根网格到装饰器
        AddVisualChild(_rootGrid);
        AddLogicalChild(_rootGrid);

        // 监听窗口大小变化
        window.SizeChanged += Window_SizeChanged;
    }

    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index)
    {
        return _rootGrid;
    }

    private void Overlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_drawer.IsOverlayClickable)
        {
            _drawer.IsOpen = false;
        }
        e.Handled = true;
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateDrawerPosition();
    }
    protected override void OnRender(DrawingContext drawingContext)
    {
        Rect adornedElementRect = new Rect(this.AdornedElement.RenderSize);

        // 绘制红色边框
        Pen renderPen = new Pen(new SolidColorBrush(Colors.Red), 1.0);
        drawingContext.DrawRectangle(null, renderPen, adornedElementRect);

        base.OnRender(drawingContext);
    }

    public void ShowDrawer()
    {
        _isDrawerOpen = true;
        _drawerContent.Visibility = Visibility.Visible;
        UpdateDrawerPosition();
    }

    private void UpdateDrawerPosition()
    {
        if (_rootGrid == null || _window == null)
            return;

        _rootGrid.Width = _window.Width;
        _rootGrid.Height = _window.Height;
        _overlay.Width = _window.Width;
        _overlay.Height = _window.Height;

        double drawerWidth = _drawerContent.Width;
        double drawerHeight = _drawerContent.Height > 0 ? _drawerContent.Height : _window.Height;

        // 清除之前的布局
        _rootGrid.Children.Remove(_drawerContent);
        _rootGrid.Children.Add(_drawerContent);

        // 根据位置设置抽屉位置
        switch (_drawer.Placement)
        {
            case DrawerPlacement.Left:
                Canvas.SetLeft(_drawerContent, 0);
                Canvas.SetTop(_drawerContent, 0);
                _drawerContent.Width = drawerWidth;
                _drawerContent.Height = drawerHeight;
                break;
            case DrawerPlacement.Right:
                Canvas.SetLeft(_drawerContent, _window.Width - drawerWidth);
                Canvas.SetTop(_drawerContent, 0);
                _drawerContent.Width = drawerWidth;
                _drawerContent.Height = drawerHeight;
                break;
            case DrawerPlacement.Top:
                Canvas.SetLeft(_drawerContent, 0);
                Canvas.SetTop(_drawerContent, 0);
                _drawerContent.Width = _window.Width;
                _drawerContent.Height = drawerHeight;
                break;
            case DrawerPlacement.Bottom:
                Canvas.SetLeft(_drawerContent, 0);
                Canvas.SetTop(_drawerContent, _window.Height - drawerHeight);
                _drawerContent.Width = _window.Width;
                _drawerContent.Height = drawerHeight;
                break;
        }

        _rootGrid.InvalidateMeasure();
        _rootGrid.InvalidateArrange();
    }

    protected override Size MeasureOverride(Size constraint)
    {
        _rootGrid.Measure(constraint);
        return _rootGrid.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _rootGrid.Arrange(new Rect(finalSize));
        return finalSize;
    }
}
