using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Threading;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

[TemplatePart(Name = BarScalePartName, Type = typeof(ScaleTransform))]
[TemplatePart(Name = PegTranslatePartName, Type = typeof(TranslateTransform))]
public class NLoadingBarIndicator : Control
{
    private const string BarScalePartName = "PART_BarScale";
    private const string PegTranslatePartName = "PART_PegTranslate";
    private readonly DispatcherTimer trickleTimer;
    private ScaleTransform? barScalePart;
    private TranslateTransform? pegTranslatePart;

    static NLoadingBarIndicator()
    {
        ElementBase.DefaultStyle<NLoadingBarIndicator>(DefaultStyleKeyProperty);
    }

    public NLoadingBarIndicator()
    {
        trickleTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(140)
        };
        trickleTimer.Tick += HandleTrickleTimerTick;
        SizeChanged += HandleSizeChanged;
        UpdateVisualState();
    }

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        private set => SetValue(ProgressProperty, value);
    }

    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(nameof(Progress), typeof(double), typeof(NLoadingBarIndicator), new PropertyMetadata(0d, OnVisualPropertyChanged));

    public bool IsError
    {
        get => (bool)GetValue(IsErrorProperty);
        private set => SetValue(IsErrorPropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsErrorPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsError), typeof(bool), typeof(NLoadingBarIndicator), new PropertyMetadata(false));

    public static readonly DependencyProperty IsErrorProperty = IsErrorPropertyKey.DependencyProperty;

    public double BarHeight
    {
        get => (double)GetValue(BarHeightProperty);
        set => SetValue(BarHeightProperty, value);
    }

    public static readonly DependencyProperty BarHeightProperty =
        ElementBase.Property<NLoadingBarIndicator, double>(nameof(BarHeightProperty), 2.5d);

    public Brush NormalBrush
    {
        get => (Brush)GetValue(NormalBrushProperty);
        set => SetValue(NormalBrushProperty, value);
    }

    public static readonly DependencyProperty NormalBrushProperty =
        ElementBase.Property<NLoadingBarIndicator, Brush>(nameof(NormalBrushProperty), Brushes.DodgerBlue);

    public Brush ErrorBrush
    {
        get => (Brush)GetValue(ErrorBrushProperty);
        set => SetValue(ErrorBrushProperty, value);
    }

    public static readonly DependencyProperty ErrorBrushProperty =
        ElementBase.Property<NLoadingBarIndicator, Brush>(nameof(ErrorBrushProperty), Brushes.IndianRed);

    public Brush CurrentBrush
    {
        get => (Brush)GetValue(CurrentBrushProperty);
        private set => SetValue(CurrentBrushPropertyKey, value);
    }

    private static readonly DependencyPropertyKey CurrentBrushPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CurrentBrush), typeof(Brush), typeof(NLoadingBarIndicator), new PropertyMetadata(Brushes.DodgerBlue));

    public static readonly DependencyProperty CurrentBrushProperty = CurrentBrushPropertyKey.DependencyProperty;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        barScalePart = GetTemplateChild(BarScalePartName) as ScaleTransform;
        pegTranslatePart = GetTemplateChild(PegTranslatePartName) as TranslateTransform;
        UpdateVisualState();
    }

    public void Start()
    {
        BeginAnimation(OpacityProperty, null);
        BeginAnimation(ProgressProperty, null);
        SetCurrentValue(OpacityProperty, 1d);
        SetValue(IsErrorPropertyKey, false);

        if (Progress <= 0d || Progress >= 1d)
        {
            Progress = 0d;
            AnimateProgressTo(0.08d, 160d);
        }

        if (!trickleTimer.IsEnabled)
        {
            trickleTimer.Start();
        }
    }

    public void Update(double progress)
    {
        BeginAnimation(OpacityProperty, null);
        SetCurrentValue(OpacityProperty, 1d);
        SetValue(IsErrorPropertyKey, false);
        AnimateProgressTo(Clamp(progress), 180d);

        if (!trickleTimer.IsEnabled && Progress < 0.92d)
        {
            trickleTimer.Start();
        }
    }

    public void Finish()
    {
        Complete(false);
    }

    public void Error()
    {
        Complete(true);
    }

    public void Reset()
    {
        trickleTimer.Stop();
        BeginAnimation(OpacityProperty, null);
        BeginAnimation(ProgressProperty, null);
        SetCurrentValue(OpacityProperty, 0d);
        SetValue(IsErrorPropertyKey, false);
        Progress = 0d;
    }

    private void Complete(bool isError)
    {
        trickleTimer.Stop();
        SetValue(IsErrorPropertyKey, isError);
        AnimateProgressTo(1d, 220d, FadeOutAndReset);
    }

    private void FadeOutAndReset()
    {
        var fadeAnimation = new DoubleAnimation
        {
            From = 1d,
            To = 0d,
            Duration = TimeSpan.FromMilliseconds(220),
            FillBehavior = FillBehavior.Stop
        };
        fadeAnimation.Completed += (_, _) => Reset();
        BeginAnimation(OpacityProperty, fadeAnimation, HandoffBehavior.SnapshotAndReplace);
    }

    private void AnimateProgressTo(double target, double durationMs, Action? completed = null)
    {
        var animation = new DoubleAnimation
        {
            To = Clamp(target),
            Duration = TimeSpan.FromMilliseconds(durationMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.HoldEnd
        };

        if (completed is not null)
        {
            animation.Completed += (_, _) => completed();
        }

        BeginAnimation(ProgressProperty, animation, HandoffBehavior.SnapshotAndReplace);
    }

    private void HandleTrickleTimerTick(object? sender, EventArgs e)
    {
        var current = Progress;
        if (current >= 0.92d)
        {
            trickleTimer.Stop();
            return;
        }

        var increment = current switch
        {
            < 0.18d => 0.14d,
            < 0.42d => 0.08d,
            < 0.68d => 0.05d,
            < 0.86d => 0.025d,
            _ => 0.012d
        };

        AnimateProgressTo(Math.Min(0.92d, current + increment), 180d);
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateVisualState();
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NLoadingBarIndicator indicator)
        {
            indicator.UpdateVisualState();
        }
    }

    private void UpdateVisualState()
    {
        if (barScalePart is not null)
        {
            barScalePart.ScaleX = Clamp(Progress);
        }

        CurrentBrush = IsError ? ErrorBrush : NormalBrush;

        if (pegTranslatePart is not null)
        {
            var pegWidth = 36d;
            var offset = Math.Max(0d, ActualWidth * Clamp(Progress) - pegWidth);
            pegTranslatePart.X = offset;
        }
    }

    private static double Clamp(double value)
    {
        return double.IsNaN(value) || double.IsInfinity(value)
            ? 0d
            : Math.Max(0d, Math.Min(1d, value));
    }
}
