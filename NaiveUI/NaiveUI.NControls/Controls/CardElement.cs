using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using NaiveUI.NControls.Themes;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.ControlsExample {
    [TemplatePart(Name = "cardBorder", Type = typeof(Border))]
    public partial class CardElement : System.Windows.Controls.Control {
        static CardElement() {
            ElementBase.DefaultStyle<CardElement>(DefaultStyleKeyProperty);
        }

        internal Border? Border;
        private bool _isHoverHandlerAttached;
        private DropShadowEffect? _shadowEffect;
        private TranslateTransform? _borderTransform;

        public CardElement() {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public override void OnApplyTemplate() {
            if (Border is not null && _isHoverHandlerAttached)
            {
                Border.MouseEnter -= HandleBorderMouseEnter;
                Border.MouseLeave -= HandleBorderMouseLeave;
                _isHoverHandlerAttached = false;
            }

            base.OnApplyTemplate();
            Border = GetTemplateChild("cardBorder") as Border;
            // 如果Hoverable属性已经是true，需要在这里初始化效果
            if (Hoverable)
            {
                ApplyHoverableEffect();
                UpdateHoverVisualState();
            }
        }

        private void ApplyHoverableEffect() {
            if (Border is null) return;

            if (_isHoverHandlerAttached)
            {
                return;
            }

            // 创建阴影效果
            _shadowEffect = new DropShadowEffect {
                BlurRadius = 10,
                Direction = 270,
                ShadowDepth = 2,
                Color = System.Windows.Media.Color.FromArgb(32, 0, 0, 0),
                Opacity = 0
            };
            Border.Effect = _shadowEffect;

            // 设置变换
            _borderTransform = new TranslateTransform();
            Border.RenderTransform = _borderTransform;

            Border.MouseEnter += HandleBorderMouseEnter;
            Border.MouseLeave += HandleBorderMouseLeave;

            _isHoverHandlerAttached = true;
            UpdateHoverVisualState();
        }

        private void RemoveHoverableEffect() {
            if (Border is null) return;

            // 移除效果和事件处理
            Border.Effect = null;
            Border.RenderTransform = null;
            Border.MouseEnter -= HandleBorderMouseEnter;
            Border.MouseLeave -= HandleBorderMouseLeave;
            Border.Background = Background;
            Border.BorderBrush = new SolidColorBrush(BoderColor);
            _isHoverHandlerAttached = false;
            _shadowEffect = null;
            _borderTransform = null;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.ThemeChanged += HandleThemeChanged;
            UpdateHoverVisualState();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.ThemeChanged -= HandleThemeChanged;
        }

        private void HandleThemeChanged(ThemeMode mode)
        {
            UpdateHoverVisualState();
        }

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateHoverVisualState();
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateHoverVisualState();
        }

        private void UpdateHoverVisualState()
        {
            if (!Hoverable || Border is null)
            {
                return;
            }

            if (IsMouseOver)
            {
                Border.Background = HoverableBackground;
                if (Application.Current?.TryFindResource("Theme.Border.Strong.Brush") is Brush hoverBorderBrush)
                {
                    Border.BorderBrush = hoverBorderBrush;
                }

                _shadowEffect?.SetCurrentValue(DropShadowEffect.OpacityProperty, 1d);
                _borderTransform?.SetCurrentValue(TranslateTransform.YProperty, -2d);
                return;
            }

            Border.Background = Background;
            Border.BorderBrush = new SolidColorBrush(BoderColor);
            _shadowEffect?.SetCurrentValue(DropShadowEffect.OpacityProperty, 0d);
            _borderTransform?.SetCurrentValue(TranslateTransform.YProperty, 0d);
        }

        private void HandleBorderMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Border is null || _shadowEffect is null || _borderTransform is null)
            {
                return;
            }

            Border.Background = HoverableBackground;
            if (Application.Current?.TryFindResource("Theme.Border.Strong.Brush") is Brush borderBrush)
            {
                Border.BorderBrush = borderBrush;
            }

            var opacityAnimation = new DoubleAnimation {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            _shadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, opacityAnimation);

            var translateAnimation = new DoubleAnimation {
                To = -2,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            _borderTransform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
            UpdateHoverVisualState();
        }

        private void HandleBorderMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Border is null || _shadowEffect is null || _borderTransform is null)
            {
                return;
            }

            Border.Background = Background;
            Border.BorderBrush = new SolidColorBrush(BoderColor);

            var opacityAnimation = new DoubleAnimation {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            _shadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, opacityAnimation);

            var translateAnimation = new DoubleAnimation {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            _borderTransform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
            UpdateHoverVisualState();
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
