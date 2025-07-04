using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace NaiveUI.NControls.ControlsExample
{
    public class AdornerContainer(UIElement adornedElement) : Adorner(adornedElement)
    {
        private UIElement _child;

        public UIElement Child
        {
            get => this._child;
            set
            {
                if (value == null)
                {
                    this.RemoveVisualChild((Visual)this._child);
                    this._child = value;
                }
                else
                {
                    this.AddVisualChild((Visual)value);
                    this._child = value;
                }
            }
        }

        protected override int VisualChildrenCount => this._child != null ? 1 : 0;

        protected override Size ArrangeOverride(Size finalSize)
        {
            this._child?.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return constraint;
        }

        protected override Visual GetVisualChild(int index)
        {
            return index == 0 && this._child != null ? (Visual)this._child : base.GetVisualChild(index);
        }
    }
}
