using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.ControlsExample;

[TemplatePart(Name = "headerBorder", Type = typeof(Border))]
[TemplatePart(Name = "ExpandSite", Type = typeof(FrameworkElement))]
public class Collapse : Control
{
    static Collapse()
    {
        //ElementBase.DefaultStyle<Collapse>(DefaultStyleKeyProperty);
    }


    private Border? _headerBorder;
    private ContentPresenter _ExpandSite;
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (this._headerBorder is not null)
        {
            this._headerBorder.MouseLeftButtonDown -= HeaderBorderChangeIsOpen;
        }
        _ExpandSite = this.GetTemplateChild("ExpandSite") as ContentPresenter;
        this._headerBorder = this.GetTemplateChild("headerBorder") as Border;

        if (this._headerBorder is not null)
        {
            this._headerBorder.MouseLeftButtonDown += HeaderBorderChangeIsOpen;
        }
        if (!IsOpen)
        {
            ChangeContentSize(false);
        }
    }

    private void HeaderBorderChangeIsOpen(object sender, MouseButtonEventArgs e)
    {
        this.IsOpen = !this.IsOpen;
        ChangeContentSize(this.IsOpen);
        //VisualStateManager.GoToElementState(this._part_Root, this.IsOpen ? "Storyboard_Expanded" : "Storyboard_Collapsed", true);
        e.Handled = true;

    }
    private double recordContentHeight;
    private void ChangeContentSize(bool isExpanded)
    {
        DoubleAnimation doubleAnimation = new DoubleAnimation();
        var height = (this._ExpandSite.Content as FrameworkElement).ActualHeight;
        if (height != 0 && recordContentHeight != height)
        {
            recordContentHeight = height;
        }
        if (isExpanded)
        {
            doubleAnimation.From = recordContentHeight;
            doubleAnimation.To = 0;
        }
        else
        {
            doubleAnimation.From = 0;           
            doubleAnimation.To = recordContentHeight;
        }
        doubleAnimation.Duration = TimeSpan.FromMilliseconds(300);
        this._ExpandSite.BeginAnimation(FrameworkElement.HeightProperty, doubleAnimation);
    }



    public static readonly DependencyProperty Header_ExtraProperty =
        ElementBase.Property<Collapse, string>(nameof(Header_ExtraProperty));

    public string Header_Extra
    {
        get => (string)GetValue(Header_ExtraProperty);
        set => SetValue(Header_ExtraProperty, value);
    }


    public static readonly DependencyProperty IsOpenProperty =
        ElementBase.Property<Collapse, bool>(nameof(IsOpenProperty), false);

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly DependencyProperty N_IconProperty =
        ElementBase.Property<Collapse, object>(nameof(N_IconProperty), new object());

    public object N_Icon
    {
        get => GetValue(N_IconProperty);
        set => SetValue(N_IconProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        ElementBase.Property<Collapse, string>(nameof(HeaderProperty));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty ContentProperty =
        ElementBase.Property<Collapse, object>(nameof(ContentProperty));

    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
}