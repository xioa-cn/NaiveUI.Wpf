using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.ControlsExample {
    [TemplatePart(Name = "cardBorder", Type = typeof(Border))]
    public partial class CardElement : System.Windows.Controls.Control {
        static CardElement() {
            ElementBase.DefaultStyle<CardElement>(DefaultStyleKeyProperty);
        }

        internal Border? Border;

        public CardElement() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            Border = GetTemplateChild("cardBorder") as Border;
            // 如果Hoverable属性已经是true，需要在这里初始化效果
            if (Hoverable)
            {
                ApplyHoverableEffect();
            }
        }

        private void ApplyHoverableEffect() {
            if (Border is null) return;

            // 创建阴影效果
            var shadowEffect = new DropShadowEffect {
                BlurRadius = 10,
                Direction = 270,
                ShadowDepth = 2,
                Color = System.Windows.Media.Color.FromArgb(32, 0, 0, 0),
                Opacity = 0
            };
            Border.Effect = shadowEffect;

            // 设置变换
            var transform = new TranslateTransform();
            Border.RenderTransform = transform;

            // 创建鼠标进入事件处理
            Border.MouseEnter += (s, args) =>
            {
                var opacityAnimation = new DoubleAnimation {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                shadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, opacityAnimation);

                var translateAnimation = new DoubleAnimation {
                    To = -2,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                transform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
            };

            // 创建鼠标离开事件处理
            Border.MouseLeave += (s, args) =>
            {
                var opacityAnimation = new DoubleAnimation {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                shadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, opacityAnimation);

                var translateAnimation = new DoubleAnimation {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                transform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
            };
        }

        private void RemoveHoverableEffect() {
            if (Border is null) return;

            // 移除效果和事件处理
            Border.Effect = null;
            Border.RenderTransform = null;
            Border.MouseEnter -= null;
            Border.MouseLeave -= null;
        }

        public object ContentElement {
            get { return (object)GetValue(ContentElementProperty); }
            set { SetValue(ContentElementProperty, value); }
        }

        public static readonly DependencyProperty ContentElementProperty =
            ElementBase.Property<CardElement, object>(nameof(ContentElementProperty), "Content");


        public object HeaderElement {
            get { return (object)GetValue(HeaderElementProperty); }
            set { SetValue(HeaderElementProperty, value); }
        }

        public static readonly DependencyProperty HeaderElementProperty =
            ElementBase.Property<CardElement, object>(nameof(HeaderElementProperty), null);


        public object? FooterElement {
            get { return (object?)GetValue(FooterElementProperty); }
            set { SetValue(FooterElementProperty, value); }
        }

        public static readonly DependencyProperty FooterElementProperty =
            ElementBase.Property<CardElement, object?>(nameof(FooterElementProperty), null);


        public System.Windows.Media.Color BoderColor {
            get { return (System.Windows.Media.Color)GetValue(BoderColorProperty); }
            set { SetValue(BoderColorProperty, value); }
        }

        public static readonly DependencyProperty BoderColorProperty =
            ElementBase.Property<CardElement, System.Windows.Media.Color>(nameof(BoderColorProperty),
                System.Windows.Media.Colors.Gray);

        public Brush FooterBackgroundBrush {
            get { return (Brush)GetValue(FooterBackgroundBrushProperty); }
            set { SetValue(FooterBackgroundBrushProperty, value); }
        }

        public static readonly DependencyProperty FooterBackgroundBrushProperty =
            ElementBase.Property<CardElement, Brush>(nameof(FooterBackgroundBrushProperty), Brushes.Gray);


        public bool OpenBordered {
            get { return (bool)GetValue(OpenBorderedProperty); }
            set { SetValue(OpenBorderedProperty, value); }
        }

        public static readonly DependencyProperty OpenBorderedProperty =
            ElementBase.Property<CardElement, bool>(nameof(OpenBorderedProperty), true);

        public bool Hoverable {
            get { return (bool)GetValue(HoverableProperty); }
            set { SetValue(HoverableProperty, value); }
        }

        public static readonly DependencyProperty HoverableProperty =
            ElementBase.Property<CardElement, bool>(nameof(HoverableProperty), false, HoverablePropertyChangedCallBack);


        public Brush HoverableBackground {
            get { return (Brush)GetValue(HoverableBackgroundProperty); }
            set { SetValue(HoverableBackgroundProperty, value); }
        }

        public static readonly DependencyProperty HoverableBackgroundProperty =
            ElementBase.Property<CardElement, Brush>(nameof(HoverableBackgroundProperty),
                new SolidColorBrush(System.Windows.Media.Color.FromRgb(250, 250, 252)));

        private static void HoverablePropertyChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue == e.OldValue) return;
            if (d is CardElement card)
            {
                var status = (bool)e.NewValue;
                if (status)
                {
                    card.ApplyHoverableEffect();
                }
                else
                {
                    card.RemoveHoverableEffect();
                }
            }
        }

        public N_Skeleton Skeleton {
            get { return (N_Skeleton)GetValue(SkeletonProperty); }
            set { SetValue(SkeletonProperty, value); }
        }

        public static readonly DependencyProperty SkeletonProperty =
            ElementBase.Property<CardElement, N_Skeleton>(nameof(SkeletonProperty), N_Skeleton.Normal);
    }

    public enum N_Skeleton {
        Normal,
        Loading
    }
}