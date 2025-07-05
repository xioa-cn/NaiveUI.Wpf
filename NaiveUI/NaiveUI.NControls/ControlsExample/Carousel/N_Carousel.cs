using NaiveUI.NControls.Tools;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace NaiveUI.NControls.ControlsExample {
    public class Carousel : Control {
        static Carousel() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Carousel),
                new FrameworkPropertyMetadata(typeof(Carousel)));
        }

        #region 命令实现

        public ICommand MovePreviousCommand { get; private set; }
        public ICommand MoveNextCommand { get; private set; }

        private void ExecuteMovePrevious(object parameter) {
            if (Items == null || Items.Count <= 1)
                return;

            // 计算前一个索引（循环处理）
            int newIndex = (CurrentIndex - 1 + Items.Count) % Items.Count;
            CurrentIndex = newIndex;
        }

        private void ExecuteMoveNext(object parameter) {
            if (Items == null || Items.Count <= 1)
                return;

            // 计算下一个索引（循环处理）
            int newIndex = (CurrentIndex + 1) % Items.Count;
            CurrentIndex = newIndex;
        }

        #endregion

        public Carousel() {
            // 初始化命令
            MovePreviousCommand = new RelayCommand(ExecuteMovePrevious);
            MoveNextCommand = new RelayCommand(ExecuteMoveNext);
            SelectSlideCommand = new RelayCommand(ExecuteSelectSlide);
            // 初始化Items集合
            Items = new ObservableCollection<object>();
            Indicators = new ObservableCollection<IndicatorItem>();

            // 初始化命令
            PreviousCommand = new RelayCommand(ExecutePreviousCommand);
            NextCommand = new RelayCommand(ExecuteNextCommand);
            GoToCommand = new RelayCommand(ExecuteGoToCommand);

            // 监听Items集合变化
            ((INotifyCollectionChanged)Items).CollectionChanged += OnItemsCollectionChanged;
        }

        #region 依赖属性

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<object>),
                typeof(Carousel), new PropertyMetadata(null));

        public static readonly DependencyProperty CurrentIndexProperty =
            DependencyProperty.Register("CurrentIndex", typeof(int),
                typeof(Carousel), new PropertyMetadata(0, OnCurrentIndexChanged));

        public static readonly DependencyProperty AutoPlayProperty =
            DependencyProperty.Register("AutoPlay", typeof(bool),
                typeof(Carousel), new PropertyMetadata(true, OnAutoPlayChanged));

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(double),
                typeof(Carousel), new PropertyMetadata(3.0, OnIntervalChanged));

        public static readonly DependencyProperty ShowIndicatorsProperty =
            DependencyProperty.Register("ShowIndicators", typeof(bool),
                typeof(Carousel), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowNavigationButtonsProperty =
            DependencyProperty.Register("ShowNavigationButtons", typeof(bool),
                typeof(Carousel), new PropertyMetadata(true));

        public static readonly DependencyProperty TransitionDurationProperty =
            DependencyProperty.Register("TransitionDuration", typeof(double),
                typeof(Carousel), new PropertyMetadata(0.5));

        #endregion

        #region 属性包装器

        public ObservableCollection<object> Items {
            get { return (ObservableCollection<object>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public int CurrentIndex {
            get { return (int)GetValue(CurrentIndexProperty); }
            set { SetValue(CurrentIndexProperty, value); }
        }

        public bool AutoPlay {
            get { return (bool)GetValue(AutoPlayProperty); }
            set { SetValue(AutoPlayProperty, value); }
        }

        public double Interval {
            get { return (double)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        public bool ShowIndicators {
            get { return (bool)GetValue(ShowIndicatorsProperty); }
            set { SetValue(ShowIndicatorsProperty, value); }
        }

        public bool ShowNavigationButtons {
            get { return (bool)GetValue(ShowNavigationButtonsProperty); }
            set { SetValue(ShowNavigationButtonsProperty, value); }
        }

        public double TransitionDuration {
            get { return (double)GetValue(TransitionDurationProperty); }
            set { SetValue(TransitionDurationProperty, value); }
        }

        #endregion

        #region 内部属性

        internal static readonly DependencyProperty IndicatorsProperty =
            ElementBase.Property<Carousel, ObservableCollection<IndicatorItem>>(
                nameof(IndicatorsProperty),
                new ObservableCollection<IndicatorItem>());

        internal ObservableCollection<IndicatorItem> Indicators {
            get => (ObservableCollection<IndicatorItem>)GetValue(IndicatorsProperty);
            set => SetValue(IndicatorsProperty, value);
        }

        internal ICommand PreviousCommand { get; private set; }
        internal ICommand NextCommand { get; private set; }
        internal ICommand GoToCommand { get; private set; }

        internal static readonly DependencyProperty SelectSlideCommandProperty =
            ElementBase.Property<Carousel, ICommand>(nameof(SelectSlideCommandProperty));

        internal ICommand SelectSlideCommand {
            get => (ICommand)GetValue(SelectSlideCommandProperty);
            set => SetValue(SelectSlideCommandProperty, value);
        }

        private DispatcherTimer _timer;
        private bool _isTransitioning;

        #endregion

        #region 方法和事件处理

        private Storyboard _currentAnimation;
        private ContentPresenter? _activePresenter;
        private ContentPresenter _inactivePresenter;

        private void OnCurrentIndex1Changed(object? sender, EventArgs e) {
            if (_activePresenter != null )
            {
                BeginTransition();
            }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            // 获取模板中的ContentPresenter
            _activePresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;

            // 初始化动画
            ApplyAnimation();

            // 监听CurrentIndex变化以触发动画
            DependencyPropertyDescriptor.FromProperty(CurrentIndexProperty, typeof(Carousel))
                .AddValueChanged(this, OnCurrentIndex1Changed);


            // 初始化定时器
            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Tick += TimerTick;
                UpdateTimer();
            }

            // 更新指示器
            UpdateIndicators();
        }

        private static void OnCurrentIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var carousel = (Carousel)d;
            carousel.OnCurrentIndexChanged((int)e.OldValue, (int)e.NewValue);
        }

        private void OnCurrentIndexChanged(int oldIndex, int newIndex) {
            if (Items == null || Items.Count == 0)
                return;

            if (newIndex < 0)
            {
                CurrentIndex = Items.Count - 1;
            }

            else if (newIndex >= Items.Count)
            {
                CurrentIndex = 0;
            }

            else
            {
                TransitionTo(newIndex);
            }


            UpdateIndicators();
        }

        private static void OnAutoPlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var carousel = (Carousel)d;
            carousel.UpdateTimer();
        }

        private static void OnIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var carousel = (Carousel)d;
            carousel.UpdateTimer();
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateIndicators();

            if (Items != null && Items.Count > 0)
            {
                // 确保CurrentIndex在有效范围内
                if (CurrentIndex >= Items.Count)
                    CurrentIndex = Items.Count - 1;

                // 如果是首次添加项目，显示第一个
                if (Items.Count == 1)
                    CurrentIndex = 0;
            }
            else
            {
                CurrentIndex = 0;
            }

            UpdateTimer();
        }

        private void UpdateIndicators() {
            if (Indicators == null)
                return;

            Indicators.Clear();

            if (Items == null)
                return;

            for (int i = 0; i < Items.Count; i++)
            {
                Indicators.Add(new IndicatorItem {
                    Index = i,
                    IsSelected = i == CurrentIndex,
                    SelectSlideCommand = SelectSlideCommand
                });
            }
        }

        private void UpdateTimer() {
            if (_timer == null)
                return;

            _timer.Stop();

            if (AutoPlay && Items != null && Items.Count > 1)
            {
                _timer.Interval = TimeSpan.FromSeconds(Interval);
                _timer.Start();
            }
        }

        private void TimerTick(object sender, EventArgs e) {
            if (_isTransitioning)
                return;

            CurrentIndex = (CurrentIndex + 1) % Items.Count;
        }


        private void ExecutePreviousCommand(object parameter) {
            if (_isTransitioning)
                return;

            CurrentIndex--;
        }

        private void ExecuteSelectSlide(object parameter) {
            if (parameter == null || Items == null || Items.Count == 0)
                return;

            // 解析参数为索引
            if (parameter is int index)
            {
                if (index >= 0 && index < Items.Count)
                    CurrentIndex = index;
            }
            else if (parameter is string indexStr && int.TryParse(indexStr, out int parsedIndex))
            {
                if (parsedIndex >= 0 && parsedIndex < Items.Count)
                    CurrentIndex = parsedIndex;
            }
            else if (parameter is IndicatorItem indicatorItem)
            {
                CurrentIndex = indicatorItem.Index;
            }
        }

        private void ExecuteNextCommand(object parameter) {
            if (_isTransitioning)
                return;

            CurrentIndex++;
        }

        private void ExecuteGoToCommand(object parameter) {
            if (_isTransitioning || parameter == null)
                return;

            if (int.TryParse(parameter.ToString(), out int index))
            {
                if (index >= 0 && index < Items.Count)
                    CurrentIndex = index;
            }
        }

        private void TransitionTo(int newIndex) {
            if (_isTransitioning || Items == null || Items.Count == 0)
                return;

            _isTransitioning = true;


            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { _isTransitioning = false; }));
        }

        #endregion


        // 动画类型：fade, slide, zoom, none
        public static readonly DependencyProperty AnimationTypeProperty =
            DependencyProperty.Register("AnimationType", typeof(string),
                typeof(Carousel), new PropertyMetadata("", OnAnimationTypeChanged));

        // 动画持续时间（秒）
        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration", typeof(double),
                typeof(Carousel), new PropertyMetadata(0.5));

        // 动画缓动函数类型
        public static readonly DependencyProperty EasingFunctionProperty =
            DependencyProperty.Register("EasingFunction", typeof(string),
                typeof(Carousel), new PropertyMetadata(""));

        // 动画方向（用于滑动动画）
        public static readonly DependencyProperty AnimationDirectionProperty =
            DependencyProperty.Register("AnimationDirection", typeof(string),
                typeof(Carousel), new PropertyMetadata("Left"));

        public string AnimationType {
            get { return (string)GetValue(AnimationTypeProperty); }
            set { SetValue(AnimationTypeProperty, value); }
        }

        public double AnimationDuration {
            get { return (double)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        public string EasingFunction {
            get { return (string)GetValue(EasingFunctionProperty); }
            set { SetValue(EasingFunctionProperty, value); }
        }

        public string AnimationDirection {
            get { return (string)GetValue(AnimationDirectionProperty); }
            set { SetValue(AnimationDirectionProperty, value); }
        }

        private static void OnAnimationTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var carousel = (Carousel)d;
            carousel.ApplyAnimation();
        }

        private void ApplyAnimation() {
            // 清除现有动画
            if (_currentAnimation != null)
            {
                _currentAnimation.Completed -= AnimationCompleted;
                _currentAnimation = null;
            }

            // 根据AnimationType创建动画
            _currentAnimation = CreateAnimation();

            if (_currentAnimation != null)
            {
                _currentAnimation.Completed += AnimationCompleted;
            }
        }

        private Storyboard CreateAnimation() {
            var storyboard = new Storyboard();

            // 设置动画持续时间
            Duration duration = TimeSpan.FromSeconds(AnimationDuration);

            // 根据动画类型添加不同的动画效果
            switch (AnimationType.ToLower())
            {
                case "fade":
                    AddFadeAnimation(storyboard, duration);
                    break;

                case "slidefromright":
                    AddSlideAnimation(storyboard, duration, 100, 0);
                    break;

                case "slidefromleft":
                    AddSlideAnimation(storyboard, duration, -100, 0);
                    break;

                case "slideup":
                    AddSlideAnimation(storyboard, duration, 0, -100, 0, 0);
                    break;

                case "slidedown":
                    AddSlideAnimation(storyboard, duration, 0, 100, 0, 0);
                    break;

                case "zoom":
                    AddZoomAnimation(storyboard, duration);
                    break;

                case "none":
                default:
                    // 无动画，立即显示
                    AddImmediateAnimation(storyboard);
                    break;
            }

            return storyboard;
        }

        private void AddFadeAnimation(Storyboard storyboard, Duration duration) {
            // 淡入动画
            var fadeIn = new DoubleAnimation {
                From = 0,
                To = 1,
                Duration = duration,
                EasingFunction = CreateEasingFunction()
            };

            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(fadeIn);
        }

        private void AddSlideAnimation(Storyboard storyboard, Duration duration,
            double fromX, double toX, double fromY = 0, double toY = 0) {
            // 设置RenderTransform
            if (_activePresenter != null && _activePresenter.RenderTransform == Transform.Identity)
            {
                _activePresenter.RenderTransform = new TranslateTransform();
            }

            // 水平滑动动画
            if (fromX != toX)
            {
                var slideX = new DoubleAnimation {
                    From = fromX,
                    To = toX,
                    Duration = duration,
                    EasingFunction = CreateEasingFunction()
                };

                Storyboard.SetTargetProperty(slideX,
                    new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                storyboard.Children.Add(slideX);
            }

            // 垂直滑动动画
            if (fromY != toY)
            {
                var slideY = new DoubleAnimation {
                    From = fromY,
                    To = toY,
                    Duration = duration,
                    EasingFunction = CreateEasingFunction()
                };

                Storyboard.SetTargetProperty(slideY,
                    new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                storyboard.Children.Add(slideY);
            }

            // 淡入动画
            var fadeIn = new DoubleAnimation {
                From = 0,
                To = 1,
                Duration = duration,
                EasingFunction = CreateEasingFunction()
            };

            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(fadeIn);
        }

        private void AddZoomAnimation(Storyboard storyboard, Duration duration) {
            // 设置RenderTransform
            if (_activePresenter != null && _activePresenter.RenderTransform == Transform.Identity)
            {
                _activePresenter.RenderTransform = new ScaleTransform();
            }

            // 缩放动画
            var scaleX = new DoubleAnimation {
                From = 0.9,
                To = 1,
                Duration = duration,
                EasingFunction = CreateEasingFunction()
            };

            var scaleY = new DoubleAnimation {
                From = 0.9,
                To = 1,
                Duration = duration,
                EasingFunction = CreateEasingFunction()
            };

            Storyboard.SetTargetProperty(scaleX,
                new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            Storyboard.SetTargetProperty(scaleY,
                new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

            storyboard.Children.Add(scaleX);
            storyboard.Children.Add(scaleY);

            // 淡入动画
            var fadeIn = new DoubleAnimation {
                From = 0,
                To = 1,
                Duration = duration,
                EasingFunction = CreateEasingFunction()
            };

            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(fadeIn);
        }

        private void AddImmediateAnimation(Storyboard storyboard) {
            // 立即显示，无动画
            var immediate = new DoubleAnimation {
                From = 1,
                To = 1,
                Duration = TimeSpan.FromSeconds(0)
            };

            Storyboard.SetTargetProperty(immediate, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(immediate);
        }
       
        private EasingFunctionBase CreateEasingFunction() {
            // 根据EasingFunction属性创建缓动函数
            switch (EasingFunction.ToLower())
            {
                case "quadraticease":
                    return new QuadraticEase { EasingMode = GetEasingMode() };

                case "cubicease":
                    return new CubicEase { EasingMode = GetEasingMode() };

                case "quarticease":
                    return new QuarticEase { EasingMode = GetEasingMode() };

                case "elastic":
                    return new ElasticEase {
                        EasingMode = GetEasingMode(),
                        Oscillations = 3,
                        Springiness = 3
                    };

                case "bounce":
                    return new BounceEase {
                        EasingMode = GetEasingMode(),
                        Bounces = 2,
                        Bounciness = 2
                    };

                default:
                    return new CubicEase { EasingMode = GetEasingMode() };
            }
        }

        private EasingMode GetEasingMode() {
            // 根据AnimationDirection确定缓动模式
            switch (AnimationDirection.ToLower())
            {
                case "easein":
                    return EasingMode.EaseIn;

                case "easeout":
                    return EasingMode.EaseOut;

                case "easeinout":
                    return EasingMode.EaseInOut;

                default:
                    return EasingMode.EaseOut;
            }
        }

        private void BeginTransition() {
            if (_currentAnimation == null || _activePresenter == null)
                return;

            // 设置初始状态
            _activePresenter.Opacity = 0;

            if (_activePresenter.RenderTransform is TranslateTransform translateTransform)
            {
                if (AnimationType.Contains("right"))
                    translateTransform.X = 100;
                else if (AnimationType.Contains("left"))
                    translateTransform.X = -100;
                else
                    translateTransform.X = 0;

                if (AnimationType.Contains("down"))
                    translateTransform.Y = 100;
                else if (AnimationType.Contains("up"))
                    translateTransform.Y = -100;
                else
                    translateTransform.Y = 0;
            }

            if (_activePresenter.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.ScaleX = 0.9;
                scaleTransform.ScaleY = 0.9;
            }
           
            Storyboard.SetTarget(_currentAnimation, _activePresenter);
            _currentAnimation.Begin();
        }

        private bool IsAnimationRunning() {
            return _currentAnimation != null && _currentAnimation.GetCurrentState() == ClockState.Active;
        }

        private void AnimationCompleted(object sender, EventArgs e) {
            // 动画完成后的清理工作
            if (_activePresenter != null)
            {
                _activePresenter.Opacity = 1;

                if (_activePresenter.RenderTransform is TranslateTransform translateTransform)
                {
                    translateTransform.X = 0;
                    translateTransform.Y = 0;
                }

                if (_activePresenter.RenderTransform is ScaleTransform scaleTransform)
                {
                    scaleTransform.ScaleX = 1;
                    scaleTransform.ScaleY = 1;
                }
            }
        }
    }
}