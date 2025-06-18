using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.ControlsExample;

[TemplatePart(Name = "canClose", Type = typeof(Button))]
public class N_Tag : System.Windows.Controls.Control {
    static N_Tag() {
        ElementBase.DefaultStyle<N_Tag>(DefaultStyleKeyProperty);
    }

    public double CloseSize {
        get { return (double)GetValue(CloseSizeProperty); }
        set { SetValue(CloseSizeProperty, value); }
    }

    public static readonly DependencyProperty CloseSizeProperty =
        ElementBase.Property<N_Tag, double>(nameof(CloseSizeProperty), 10);

    private Button? _closeButton;

    public override void OnApplyTemplate() {
        base.OnApplyTemplate();
        if (_closeButton != null)
        {
            _closeButton.Click -= CloseButton_Click;
        }

        _closeButton = GetTemplateChild("canClose") as Button;
        // 获取模板中的关闭按钮
        if (_closeButton != null)
        {
            _closeButton.Click += CloseButton_Click;
        }

        this.PreviewMouseDown += N_Tag_PreviewMouseDown;
    }

    private void N_Tag_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
        if (this.OpenChecked is not null && (bool)this.OpenChecked)
        {
            this.Checked = !this.Checked;
            e.Handled = true;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) {
        if (this._closeButton is null)
        {
            throw new Exception();
        }

        if (!this.Closeable)
        {
            return;
        }

        if (this.CloseCommandParameter is not null)
        {
            _closeButton.CommandParameter = this.CloseCommandParameter;
        }

        if (_closeButton.Command is null && this.CloseCommand is not null)
        {
            _closeButton.Command = this.CloseCommand;
        }


        //_closeButton.Command?.Execute(this.CloseCommandParameter);
        // 防止事件继续向上冒泡
        e.Handled = true;

        // 创建并触发自定义 Click 事件
        var clickArgs = new RoutedEventArgs(CloseClickEvent, this);
        RaiseEvent(clickArgs);
    }

    public static readonly RoutedEvent CloseClickEvent =
        ElementBase.RoutedEvent<N_Tag, RoutedEventHandler>(nameof(CloseClickEvent));

    public event RoutedEventHandler CloseClick {
        add { AddHandler(CloseClickEvent, value); }
        remove { RemoveHandler(CloseClickEvent, value); }
    }

    public bool Closeable {
        get { return (bool)GetValue(CloseableProperty); }
        set { SetValue(CloseableProperty, value); }
    }

    public static readonly DependencyProperty CloseableProperty =
        ElementBase.Property<N_Tag, bool>(nameof(CloseableProperty), false);

    public bool Bordered {
        get { return (bool)GetValue(BorderedProperty); }
        set { SetValue(BorderedProperty, value); }
    }

    public static readonly DependencyProperty BorderedProperty =
        ElementBase.Property<N_Tag, bool>(nameof(BorderedProperty), true);

    public CornerRadius CornerRadius {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        ElementBase.Property<N_Tag, CornerRadius>(nameof(CornerRadiusProperty), new CornerRadius(1));

    public Grade Grade {
        get { return (Grade)GetValue(GradeProperty); }
        set { SetValue(GradeProperty, value); }
    }

    public static readonly DependencyProperty GradeProperty =
        ElementBase.Property<N_Tag, Grade>(nameof(GradeProperty), Grade.Default);

    public string Text {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public static readonly DependencyProperty TextProperty =
        ElementBase.Property<N_Tag, string>(nameof(TextProperty), "N_Tag");

    public static readonly DependencyProperty CloseCommandProperty =
        ElementBase.Property<N_Tag, ICommand>(nameof(CloseCommandProperty));

    public static readonly DependencyProperty CloseCommandParameterProperty =
        ElementBase.Property<N_Tag, object?>(nameof(CloseCommandParameterProperty), null);

    public ICommand? CloseCommand {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public object CloseCommandParameter {
        get => GetValue(CloseCommandParameterProperty);
        set => SetValue(CloseCommandParameterProperty, value);
    }

    public bool? Checked {
        get { return (bool?)GetValue(CheckedProperty); }
        set { SetValue(CheckedProperty, value); }
    }

    public static readonly DependencyProperty CheckedProperty = ElementBase.Property<N_Tag, bool?>(
        nameof(CheckedProperty), null);

    public bool? OpenChecked {
        get { return (bool?)GetValue(OpenCheckedProperty); }
        set { SetValue(OpenCheckedProperty, value); }
    }

    public static readonly DependencyProperty OpenCheckedProperty = ElementBase.Property<N_Tag, bool?>(
        nameof(OpenCheckedProperty), null);

    public bool Round {
        get { return (bool)GetValue(RoundProperty); }
        set { SetValue(RoundProperty, value); }
    }

    public static readonly DependencyProperty RoundProperty =
        ElementBase.Property<N_Tag, bool>(nameof(RoundProperty), false);

    public object Icon {
        get { return GetValue(IconProperty); }
        set { SetValue(IconProperty, value); }
    }

    public static readonly DependencyProperty IconProperty =
        ElementBase.Property<N_Tag, object?>(nameof(IconProperty), null);

    public bool OpenIcon {
        set { SetValue(OpenIconProperty, value); }
        get { return (bool)GetValue(OpenIconProperty); }
    }

    public static readonly DependencyProperty OpenIconProperty =
        ElementBase.Property<N_Tag, bool>(nameof(OpenIconProperty), false);
}