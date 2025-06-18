using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.ControlsExample {
    /// <summary>
    /// 间距容器 - 在子元素之间添加统一的间距
    /// </summary>
    public class N_Space : StackPanel {
        #region 静态构造函数

        static N_Space() {
            ElementBase.DefaultStyle<N_Space>(DefaultStyleKeyProperty);
        }

        #endregion

        #region 依赖属性

        // 子元素之间的间距
        public static readonly DependencyProperty SpaceProperty =
            DependencyProperty.Register(
                nameof(Space),
                typeof(double),
                typeof(N_Space),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));
           

        public double Space {
            get { return (double)GetValue(SpaceProperty); }
            set { SetValue(SpaceProperty, value); }
        }

        #endregion

        #region 重写方法

        protected override Size MeasureOverride(Size constraint) {
            double space = Space;
            Orientation orientation = Orientation;
            Size stackDesiredSize = new Size();
            UIElementCollection children = InternalChildren;
            Size childConstraint = new Size(
                orientation == Orientation.Horizontal
                    ? constraint.Width - space * (children.Count - 1)
                    : constraint.Width,
                orientation == Orientation.Vertical
                    ? constraint.Height - space * (children.Count - 1)
                    : constraint.Height);

            double maxWidth = 0;
            double maxHeight = 0;
            double totalWidth = 0;
            double totalHeight = 0;

            // 测量所有子元素
            for (int i = 0; i < children.Count; i++)
            {
                UIElement child = children[i];
                if (child == null) continue;

                child.Measure(childConstraint);
                Size childDesiredSize = child.DesiredSize;

                if (orientation == Orientation.Horizontal)
                {
                    totalWidth += childDesiredSize.Width;
                    maxHeight = Math.Max(maxHeight, childDesiredSize.Height);
                }
                else
                {
                    totalHeight += childDesiredSize.Height;
                    maxWidth = Math.Max(maxWidth, childDesiredSize.Width);
                }
            }

            // 计算总间距
            double totalSpacing = space * Math.Max(0, children.Count - 1);

            // 计算最终大小
            stackDesiredSize.Width = orientation == Orientation.Horizontal ? totalWidth + totalSpacing : maxWidth;
            stackDesiredSize.Height = orientation == Orientation.Vertical ? totalHeight + totalSpacing : maxHeight;

            return stackDesiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize) {
            double space = Space;
            Orientation orientation = Orientation;
            UIElementCollection children = InternalChildren;
            Rect childBounds = new Rect(0, 0, arrangeSize.Width, arrangeSize.Height);
            double accumulatedOffset = 0;

            // 排列所有子元素
            for (int i = 0; i < children.Count; i++)
            {
                UIElement child = children[i];
                if (child == null) continue;

                if (orientation == Orientation.Horizontal)
                {
                    childBounds.X = accumulatedOffset;
                    accumulatedOffset += child.DesiredSize.Width + space;
                    childBounds.Width = child.DesiredSize.Width;
                    childBounds.Height = Math.Max(arrangeSize.Height, child.DesiredSize.Height);
                }
                else
                {
                    childBounds.Y = accumulatedOffset;
                    accumulatedOffset += child.DesiredSize.Height + space;
                    childBounds.Height = child.DesiredSize.Height;
                    childBounds.Width = Math.Max(arrangeSize.Width, child.DesiredSize.Width);
                }

                child.Arrange(childBounds);
            }

            return arrangeSize;
        }

        #endregion
    }
}