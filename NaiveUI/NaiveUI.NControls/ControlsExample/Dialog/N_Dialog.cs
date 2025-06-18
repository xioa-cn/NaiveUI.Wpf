using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media.Effects;

namespace NaiveUI.NControls.ControlsExample;

/// <summary>
/// 类似HandyControl的对话框控件
/// </summary>
public class N_Dialog : ContentControl {
    static N_Dialog() {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(N_Dialog), new FrameworkPropertyMetadata(typeof(N_Dialog)));
    }

    public N_Dialog() {
        Loaded += N_Dialog_Loaded;
        Unloaded += N_Dialog_Unloaded;
    }

    private DialogAdorner _dialogAdorner;
    private AdornerLayer _adornerLayer;

    private void N_Dialog_Loaded(object sender, RoutedEventArgs e) {
        // 获取当前窗口的AdornerLayer
        Window window = Window.GetWindow(this);
        if (window == null) return;

        _adornerLayer = AdornerLayer.GetAdornerLayer(window);
        if (_adornerLayer == null) return;

        // 创建对话框装饰器
        if (_dialogAdorner == null)
        {
            _dialogAdorner = new DialogAdorner(window, this);
            _adornerLayer.Add(_dialogAdorner);
        }
    }

    private void N_Dialog_Unloaded(object sender, RoutedEventArgs e) {
        // 清理资源
        if (_dialogAdorner != null && _adornerLayer != null)
        {
            _adornerLayer.Remove(_dialogAdorner);
            _dialogAdorner = null;
        }

        _adornerLayer = null;
    }

    #region 依赖属性

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(N_Dialog),
            new PropertyMetadata(false, OnIsOpenChanged));

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is N_Dialog dialog)
        {
            dialog._dialogAdorner?.ToggleVisibility((bool)e.NewValue);
        }
    }

    public bool IsOpen {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly DependencyProperty WidthProperty =
        DependencyProperty.Register(nameof(Width), typeof(double), typeof(N_Dialog),
            new PropertyMetadata(320.0));

    public new double Width {
        get => (double)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    public static readonly DependencyProperty HeightProperty =
        DependencyProperty.Register(nameof(Height), typeof(double), typeof(N_Dialog),
            new PropertyMetadata(double.NaN));

    public new double Height {
        get => (double)GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(N_Dialog),
            new PropertyMetadata("对话框"));

    public string Title {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty ShowTitleProperty =
        DependencyProperty.Register(nameof(ShowTitle), typeof(bool), typeof(N_Dialog),
            new PropertyMetadata(true));

    public bool ShowTitle {
        get => (bool)GetValue(ShowTitleProperty);
        set => SetValue(ShowTitleProperty, value);
    }

    public static readonly DependencyProperty IsCancelableProperty =
        DependencyProperty.Register(nameof(IsCancelable), typeof(bool), typeof(N_Dialog),
            new PropertyMetadata(true));

    public bool IsCancelable {
        get => (bool)GetValue(IsCancelableProperty);
        set => SetValue(IsCancelableProperty, value);
    }

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(nameof(AnimationDuration), typeof(TimeSpan), typeof(N_Dialog),
            new PropertyMetadata(TimeSpan.FromMilliseconds(300)));

    public TimeSpan AnimationDuration {
        get => (TimeSpan)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    #endregion

    #region 事件

    public event EventHandler<DialogResultEventArgs> Closed;

    protected virtual void OnClosed(DialogResultEventArgs e) {
        Closed?.Invoke(this, e);
    }

    public void Close(DialogResult result = DialogResult.Cancel) {
        IsOpen = false;
        OnClosed(new DialogResultEventArgs(result));
    }

    #endregion
}

/// <summary>
/// 对话框装饰器
/// </summary>
public class DialogAdorner : Adorner {
    private readonly Window _window;
    private readonly N_Dialog _dialog;
    private readonly Grid _rootGrid;
    private readonly Rectangle _overlay;
    private readonly Border _dialogBorder;
    private readonly Grid _dialogContentGrid;
    private readonly ContentControl _titleBar;
    private readonly ContentControl _contentControl;
    private readonly StackPanel _buttonPanel;
    private bool _isOpen;

    public DialogAdorner(Window window, N_Dialog dialog) : base(window) {
        _window = window;
        _dialog = dialog;
        _isOpen = false;

        // 初始化根网格
        _rootGrid = new Grid {
            Width = window.Width,
            Height = window.Height,
            Background = Brushes.Transparent
        };

        // 初始化遮罩层
        _overlay = new Rectangle {
            Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
            Width = double.PositiveInfinity,
            Height = double.PositiveInfinity,
            IsHitTestVisible = true
        };
        _overlay.MouseLeftButtonDown += Overlay_MouseLeftButtonDown;

        // 初始化对话框边框
        _dialogBorder = new Border {
            Background = Brushes.White,
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            SnapsToDevicePixels = true,
            Effect = new DropShadowEffect {
                BlurRadius = 10,
                ShadowDepth = 0,
                Color = Color.FromArgb(128, 0, 0, 0)
            }
        };

        // 初始化对话框内容网格
        _dialogContentGrid = new Grid {
            Width = _dialog.Width,
            MaxHeight = window.Height * 0.8,
            Margin = new Thickness(2)
        };

        // 初始化标题栏
        _titleBar = new ContentControl {
            Content = _dialog.Title,
            Background = Brushes.White,
            Padding = new Thickness(12, 8, 12, 8),
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            Template = GetTitleBarTemplate()
        };

        // 初始化内容区域
        _contentControl = new ContentControl {
            Content = _dialog.Content,
            Padding = new Thickness(16),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };

        // 初始化按钮面板
        _buttonPanel = new StackPanel {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 12, 16, 16),
           
        };

        // 添加关闭按钮
        var closeButton = new Button {
            Content = "关闭",
            Width = 80,
            Height = 32,
            Margin = new Thickness(0, 0, 0, 0)
        };
        closeButton.Click += (s, e) => _dialog.Close(DialogResult.Cancel);
        _buttonPanel.Children.Add(closeButton);

        // 组装对话框内容
        int rowCount = _dialog.ShowTitle ? 3 : 2;
        _dialogContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
        _dialogContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _dialogContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });

        if (_dialog.ShowTitle)
        {
            _dialogContentGrid.Children.Add(_titleBar);
            Grid.SetRow(_titleBar, 0);
        }

        _dialogContentGrid.Children.Add(_contentControl);
        Grid.SetRow(_contentControl, _dialog.ShowTitle ? 1 : 0);

        _dialogContentGrid.Children.Add(_buttonPanel);
        Grid.SetRow(_buttonPanel, rowCount - 1);

        // 将对话框添加到边框
        _dialogBorder.Child = _dialogContentGrid;

        // 添加子元素到根网格
        _rootGrid.Children.Add(_overlay);
        _rootGrid.Children.Add(_dialogBorder);

        // 添加根网格到装饰器
        AddVisualChild(_rootGrid);
        AddLogicalChild(_rootGrid);

        // 监听窗口大小变化
        window.SizeChanged += Window_SizeChanged;
    }

    private ControlTemplate GetTitleBarTemplate() {
        return new ControlTemplate(typeof(ContentControl)) {
            // VisualTree = new FrameworkElementFactory(typeof(Border), new FrameworkElementFactory(typeof(Grid))),
        
        };
    }

    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index) {
        return _rootGrid;
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
        UpdateDialogPosition();
    }

    private void Overlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if (_dialog.IsCancelable)
        {
            _dialog.Close(DialogResult.Cancel);
        }

        e.Handled = true;
    }

    public void ToggleVisibility(bool isOpen) {
        _isOpen = isOpen;
        if (isOpen)
        {
            _rootGrid.Visibility = Visibility.Visible;
            AnimateShow();
        }
        else
        {
            AnimateHide();
        }
    }

    private void AnimateShow() {
        UpdateDialogPosition();

        // 初始状态 - 缩小并透明
        _dialogBorder.RenderTransform = new ScaleTransform { ScaleX = 0.8, ScaleY = 0.8 };
        _dialogBorder.Opacity = 0;

        // 创建动画
        var scaleAnimation = new DoubleAnimation {
            From = 0.8,
            To = 1.0,
            Duration = _dialog.AnimationDuration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var opacityAnimation = new DoubleAnimation {
            From = 0,
            To = 1,
            Duration = _dialog.AnimationDuration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        // 开始动画
        (_dialogBorder.RenderTransform as ScaleTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        (_dialogBorder.RenderTransform as ScaleTransform).BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        _dialogBorder.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
    }

    private void AnimateHide() {
        if (_dialogBorder.RenderTransform == null)
        {
            _dialogBorder.RenderTransform = new ScaleTransform { ScaleX = 1.0, ScaleY = 1.0 };
        }

        // 创建动画
        var scaleAnimation = new DoubleAnimation {
            From = 1.0,
            To = 0.8,
            Duration = _dialog.AnimationDuration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        var opacityAnimation = new DoubleAnimation {
            From = 1,
            To = 0,
            Duration = _dialog.AnimationDuration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        // 动画完成后隐藏
        scaleAnimation.Completed += (s, e) => _rootGrid.Visibility = Visibility.Collapsed;
        opacityAnimation.Completed += (s, e) => _rootGrid.Visibility = Visibility.Collapsed;

        // 开始动画
        (_dialogBorder.RenderTransform as ScaleTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        (_dialogBorder.RenderTransform as ScaleTransform).BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        _dialogBorder.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
    }

    private void UpdateDialogPosition() {
        if (_rootGrid == null || _window == null)
            return;

        _rootGrid.Width = _window.Width;
        _rootGrid.Height = _window.Height;
        _overlay.Width = _window.Width;
        _overlay.Height = _window.Height;

        // 对话框居中
        double dialogWidth = _dialog.Width;
        double dialogHeight = _dialogContentGrid.DesiredSize.Height;

        if (double.IsNaN(_dialog.Height))
        {
            dialogHeight = _dialogContentGrid.DesiredSize.Height;
        }
        else
        {
            dialogHeight = _dialog.Height;
            _dialogContentGrid.Height = dialogHeight;
        }

        Canvas.SetLeft(_dialogBorder, (_window.Width - dialogWidth) / 2);
        Canvas.SetTop(_dialogBorder, (_window.Height - dialogHeight) / 2);

        _rootGrid.InvalidateMeasure();
        _rootGrid.InvalidateArrange();
    }

    protected override Size MeasureOverride(Size constraint) {
        _rootGrid.Measure(constraint);
        return _rootGrid.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize) {
        _rootGrid.Arrange(new Rect(finalSize));
        return finalSize;
    }
}

/// <summary>
/// 对话框结果
/// </summary>
public enum DialogResult {
    None,
    OK,
    Cancel
}

/// <summary>
/// 对话框结果事件参数
/// </summary>
public class DialogResultEventArgs : EventArgs {
    public DialogResult Result { get; }

    public DialogResultEventArgs(DialogResult result) {
        Result = result;
    }
}