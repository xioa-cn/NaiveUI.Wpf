using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.ControlsExample;

[TemplatePart(Name = "afterbtn", Type = typeof(Button))]
[TemplatePart(Name = "nowbtn", Type = typeof(Button))]
[TemplatePart(Name = "beforebtn", Type = typeof(Button))]
public class N_Calendar : Control
{
    private Button? afterBtn;
    private Button? nowBtn;
    private Button? beforeBtn;

    internal void SelectChangedEvent(DayInfo dayInfo) {
        this.SelectedDate = dayInfo.Date;

        RoutedEventArgs args = new RoutedEventArgs(ClickEvent, this);
        RaiseEvent(args);

        this.Command?.Execute(this.CommandParameter);
    }
    
    public static readonly RoutedEvent ClickEvent =
        ElementBase.RoutedEvent<N_Calendar, RoutedEventHandler>(nameof(ClickEvent));

    public event RoutedEventHandler Click
    {
        add { AddHandler(ClickEvent, value); }
        remove { RemoveHandler(ClickEvent, value); }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (afterBtn is not null)
            afterBtn.Click -= AfterBtnOnClick;
        if (nowBtn is not null)
            nowBtn.Click -= NowBtnOnClick;
        if (beforeBtn is not null)
            beforeBtn.Click -= BeforeBtnOnClick;
        
        afterBtn = GetTemplateChild("afterbtn") as Button;
        nowBtn = GetTemplateChild("nowbtn") as Button;
        beforeBtn = GetTemplateChild("beforebtn") as Button;
        
        if (afterBtn is not null)
            afterBtn.Click += AfterBtnOnClick;
        if (nowBtn is not null)
            nowBtn.Click += NowBtnOnClick;
        if (beforeBtn is not null)
            beforeBtn.Click += BeforeBtnOnClick;
    }   

    private void BeforeBtnOnClick(object sender, RoutedEventArgs e)
    {
        BeforeMonth();
    }

    private void NowBtnOnClick(object sender, RoutedEventArgs e)
    {
        NowDay();
    }

    public void NowDay()
    {
        this.Time = DateTime.Now;
    }

    private void AfterBtnOnClick(object sender, RoutedEventArgs e)
    {
        AfterMonth();
    }

    public void AfterMonth()
    {
        this.Time = this.Time.AddMonths(1);
    }

    public void BeforeMonth()
    {
        this.Time = this.Time.AddMonths(-1);
    }

    static N_Calendar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(N_Calendar),
            new FrameworkPropertyMetadata(typeof(N_Calendar)));
    }

    public DateTime Time
    {
        get { return (DateTime)GetValue(TimeProperty); }
        set { SetValue(TimeProperty, value); }
    }

    public static readonly DependencyProperty TimeProperty =
        ElementBase.Property<N_Calendar, DateTime>(nameof(TimeProperty), DateTime.Now, setTimeCallBack);

    private static void setTimeCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not N_Calendar calendar) return;
        if (e.NewValue is DateTime newTime)
        {
            calendar.Year = newTime.Year;
            calendar.Month = newTime.Month;
        }
    }

    public int Year
    {
        get { return (int)GetValue(YearProperty); }
        set { SetValue(YearProperty, value); }
    }

    public static readonly DependencyProperty YearProperty =
        ElementBase.Property<N_Calendar, int>(nameof(YearProperty), DateTime.Now.Year);

    public int Month
    {
        get { return (int)GetValue(MonthProperty); }
        set { SetValue(MonthProperty, value); }
    }

    public static readonly DependencyProperty MonthProperty =
        ElementBase.Property<N_Calendar, int>(nameof(MonthProperty), DateTime.Now.Month);


    public bool ShowMenu
    {
        get { return (bool)GetValue(ShowMenuProperty); }
        set { SetValue(ShowMenuProperty, value); }
    }

    public static readonly DependencyProperty ShowMenuProperty =
        ElementBase.Property<N_Calendar, bool>(nameof(ShowMenuProperty), true);


    public static readonly DependencyProperty CommandProperty =
        ElementBase.Property<N_Calendar, ICommand>(nameof(CommandProperty));

    public static readonly DependencyProperty CommandParameterProperty =
        ElementBase.Property<N_Calendar, object?>(nameof(CommandParameterProperty), null);

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public DateTime SelectedDate
    {
        get { return (DateTime)GetValue(SelectedDateProperty); }
        set { SetValue(SelectedDateProperty, value); }
    }

    public static readonly DependencyProperty SelectedDateProperty =
        ElementBase.Property<N_Calendar, DateTime>(nameof(SelectedDateProperty), DateTime.Now);
}