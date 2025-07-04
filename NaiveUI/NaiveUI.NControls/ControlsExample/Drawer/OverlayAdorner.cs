using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NaiveUI.NControls.ControlsExample;

public class OverlayAdorner : Adorner
{
    private readonly Rectangle _overlayRect;
    public OverlayAdorner(UIElement adornedElement)
        : base(adornedElement)
    {
        _overlayRect = new Rectangle
        {
            Fill = new SolidColorBrush(Color.FromArgb(128, 128, 128, 128)),
            IsHitTestVisible = true
        };
        this.AddVisualChild(_overlayRect);
        this.AddLogicalChild(_overlayRect);
        _overlayRect.MouseLeftButtonDown += OverlayAdorner_MouseLeftButtonDown;
    }

    private void OverlayAdorner_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        MouseLeftAction?.Invoke();
        e.Handled = true;
    }

    public Action? MouseLeftAction { get; set; }

    protected override Size MeasureOverride(Size constraint)
    {
        _overlayRect.Measure(constraint);
        return constraint;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _overlayRect.Arrange(new Rect(finalSize));
        return finalSize;
    }
    protected override int VisualChildrenCount => 1;
    protected override Visual GetVisualChild(int index) => _overlayRect;
}

