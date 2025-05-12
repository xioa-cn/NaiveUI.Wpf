using NaiveUI.NControls.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NaiveUI.NControls.ControlsExample
{
    /// <summary>
    /// 开关控件，继承自RadioButton以支持互斥选择功能
    /// </summary>
    public class N_Switch : RadioButton
    {
        static N_Switch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(N_Switch), new FrameworkPropertyMetadata(typeof(N_Switch)));
        }

        #region 依赖属性

        /// <summary>
        /// 获取或设置是否处于加载状态
        /// </summary>
        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        /// <summary>
        /// Loading依赖属性
        /// </summary>
        public static readonly DependencyProperty LoadingProperty =
            ElementBase.Property<N_Switch, bool>(nameof(LoadingProperty), false);

        /// <summary>
        /// 获取或设置是否选中状态
        /// </summary>
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        /// <summary>
        /// IsChecked依赖属性，用于控制开关状态
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty =
            ElementBase.Property<N_Switch, bool>(nameof(IsCheckedProperty), false, OnIsCheckedChanged);


        /// <summary>
        /// 获取或设置选中状态的颜色
        /// </summary>
        public Brush CheckedColor
        {
            get { return (Brush)GetValue(CheckedColorProperty); }
            set { SetValue(CheckedColorProperty, value); }
        }

        /// <summary>
        /// CheckedColor依赖属性，默认为#18a058
        /// </summary>
        public static readonly DependencyProperty CheckedColorProperty =
            ElementBase.Property<N_Switch, Brush>(nameof(CheckedColorProperty), new SolidColorBrush((Color)ColorConverter.ConvertFromString("#18a058")));


        /// <summary>
        /// 获取或设置轨道宽度
        /// </summary>
        public double RailWidth
        {
            get { return (double)GetValue(RailWidthProperty); }
            set { SetValue(RailWidthProperty, value); }
        }

        /// <summary>
        /// RailWidth依赖属性，默认值为40
        /// </summary>
        public static readonly DependencyProperty RailWidthProperty =
            ElementBase.Property<N_Switch, double>(nameof(RailWidthProperty), 40.0);

        /// <summary>
        /// 获取或设置轨道高度
        /// </summary>
        public double RailHeight
        {
            get { return (double)GetValue(RailHeightProperty); }
            set { SetValue(RailHeightProperty, value); }
        }

        /// <summary>
        /// RailHeight依赖属性，默认值为20
        /// </summary>
        public static readonly DependencyProperty RailHeightProperty =
           ElementBase.Property<N_Switch, double>(nameof(RailHeightProperty), 20.0);

        #endregion

        /// <summary>
        /// 重写点击事件，在加载或禁用状态下不响应点击
        /// </summary>
        protected override void OnClick()
        {
            if (!Loading && !IsEnabled)
            {
                return;
            }
            base.OnClick();
        }

        // 开启和关闭状态的动画故事板
        private Storyboard? onStoryboard;
        private Storyboard? offStoryboard;

        /// <summary>
        /// 当模板应用时初始化动画
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var thumb = GetTemplateChild("Thumb") as Border;
            if (thumb != null)
            {
                // 计算滑块移动距离
                var translateDistance = RailWidth - RailHeight;

                // 创建开启动画
                onStoryboard = new Storyboard();
                var onAnimation = new DoubleAnimation
                {
                    To = translateDistance,
                    Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTarget(onAnimation, thumb);
                Storyboard.SetTargetProperty(onAnimation, new PropertyPath("(Border.RenderTransform).(TranslateTransform.X)"));
                onStoryboard.Children.Add(onAnimation);

                // 创建关闭动画
                offStoryboard = new Storyboard();
                var offAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTarget(offAnimation, thumb);
                Storyboard.SetTargetProperty(offAnimation, new PropertyPath("(Border.RenderTransform).(TranslateTransform.X)"));
                offStoryboard.Children.Add(offAnimation);



                if (IsChecked)
                {
                    onStoryboard?.Begin();
                }
                else
                {
                    offStoryboard?.Begin();
                }

            }
        }

        /// <summary>
        /// 构造函数，注册点击事件处理
        /// </summary>
        public N_Switch()
        {
            this.Click += N_Switch_Click;
        }

        /// <summary>
        /// 点击事件处理，切换开关状态
        /// </summary>
        private void N_Switch_Click(object sender, RoutedEventArgs e)
        {
            this.IsChecked = !this.IsChecked;
        }

        /// <summary>
        /// IsChecked属性变化时的回调，处理动画播放
        /// </summary>
        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue) return;
            if (d is N_Switch switchControl)
            {
                // 根据新状态播放相应的动画
                if ((bool)e.NewValue == true)
                {
                    switchControl.onStoryboard?.Begin();
                }
                else
                {
                    switchControl.offStoryboard?.Begin();
                }
            }
        }
    }
}
