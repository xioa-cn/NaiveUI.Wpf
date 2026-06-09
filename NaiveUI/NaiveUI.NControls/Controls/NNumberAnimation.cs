using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NNumberAnimation : TextBlock
{
    private DateTime animationStartTime;
    private bool isRenderingSubscribed;
    private bool hasLoaded;
    private double animationFrom;
    private double animationTo;
    private double lastRenderedValue;

    static NNumberAnimation()
    {
        ElementBase.DefaultStyle<NNumberAnimation>(DefaultStyleKeyProperty);
    }

    public NNumberAnimation()
    {
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        CurrentValue = From;
        UpdateDisplayText();
    }

    public double To
    {
        get => (double)GetValue(ToProperty);
        set => SetValue(ToProperty, value);
    }

    public static readonly DependencyProperty ToProperty =
        ElementBase.Property<NNumberAnimation, double>(nameof(ToProperty), 0d, OnAnimationInputPropertyChanged);

    public double From
    {
        get => (double)GetValue(FromProperty);
        set => SetValue(FromProperty, value);
    }

    public static readonly DependencyProperty FromProperty =
        ElementBase.Property<NNumberAnimation, double>(nameof(FromProperty), 0d, OnAnimationInputPropertyChanged);

    public bool Active
    {
        get => (bool)GetValue(ActiveProperty);
        set => SetValue(ActiveProperty, value);
    }

    public static readonly DependencyProperty ActiveProperty =
        ElementBase.Property<NNumberAnimation, bool>(nameof(ActiveProperty), true, OnActivePropertyChanged);

    public double Duration
    {
        get => (double)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public static readonly DependencyProperty DurationProperty =
        ElementBase.Property<NNumberAnimation, double>(nameof(DurationProperty), 2000d);

    public int Precision
    {
        get => (int)GetValue(PrecisionProperty);
        set => SetValue(PrecisionProperty, value);
    }

    public static readonly DependencyProperty PrecisionProperty =
        ElementBase.Property<NNumberAnimation, int>(nameof(PrecisionProperty), 0, OnFormatPropertyChanged);

    public bool ShowSeparator
    {
        get => (bool)GetValue(ShowSeparatorProperty);
        set => SetValue(ShowSeparatorProperty, value);
    }

    public static readonly DependencyProperty ShowSeparatorProperty =
        ElementBase.Property<NNumberAnimation, bool>(nameof(ShowSeparatorProperty), false, OnFormatPropertyChanged);

    public string? Locale
    {
        get => (string?)GetValue(LocaleProperty);
        set => SetValue(LocaleProperty, value);
    }

    public static readonly DependencyProperty LocaleProperty =
        ElementBase.Property<NNumberAnimation, string?>(nameof(LocaleProperty), null, OnFormatPropertyChanged);

    public string Prefix
    {
        get => (string)GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    public static readonly DependencyProperty PrefixProperty =
        ElementBase.Property<NNumberAnimation, string>(nameof(PrefixProperty), string.Empty, OnFormatPropertyChanged);

    public string Suffix
    {
        get => (string)GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public static readonly DependencyProperty SuffixProperty =
        ElementBase.Property<NNumberAnimation, string>(nameof(SuffixProperty), string.Empty, OnFormatPropertyChanged);

    public string? FormatString
    {
        get => (string?)GetValue(FormatStringProperty);
        set => SetValue(FormatStringProperty, value);
    }

    public static readonly DependencyProperty FormatStringProperty =
        ElementBase.Property<NNumberAnimation, string?>(nameof(FormatStringProperty), null, OnFormatPropertyChanged);

    public bool IsAnimationEnabled
    {
        get => (bool)GetValue(IsAnimationEnabledProperty);
        set => SetValue(IsAnimationEnabledProperty, value);
    }

    public static readonly DependencyProperty IsAnimationEnabledProperty =
        ElementBase.Property<NNumberAnimation, bool>(nameof(IsAnimationEnabledProperty), true, OnAnimationInputPropertyChanged);

    public bool RestartOnValueChanged
    {
        get => (bool)GetValue(RestartOnValueChangedProperty);
        set => SetValue(RestartOnValueChangedProperty, value);
    }

    public static readonly DependencyProperty RestartOnValueChangedProperty =
        ElementBase.Property<NNumberAnimation, bool>(nameof(RestartOnValueChangedProperty), true);

    public bool UseCurrentValueAsFromOnChange
    {
        get => (bool)GetValue(UseCurrentValueAsFromOnChangeProperty);
        set => SetValue(UseCurrentValueAsFromOnChangeProperty, value);
    }

    public static readonly DependencyProperty UseCurrentValueAsFromOnChangeProperty =
        ElementBase.Property<NNumberAnimation, bool>(nameof(UseCurrentValueAsFromOnChangeProperty), false);

    public IEasingFunction? EasingFunction
    {
        get => (IEasingFunction?)GetValue(EasingFunctionProperty);
        set => SetValue(EasingFunctionProperty, value);
    }

    public static readonly DependencyProperty EasingFunctionProperty =
        ElementBase.Property<NNumberAnimation, IEasingFunction?>(nameof(EasingFunctionProperty), null);

    public double CurrentValue
    {
        get => (double)GetValue(CurrentValueProperty);
        private set => SetValue(CurrentValuePropertyKey, value);
    }

    private static readonly DependencyPropertyKey CurrentValuePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(CurrentValue),
            typeof(double),
            typeof(NNumberAnimation),
            new PropertyMetadata(0d, OnCurrentValuePropertyChanged));

    public static readonly DependencyProperty CurrentValueProperty = CurrentValuePropertyKey.DependencyProperty;

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        private set => SetValue(DisplayTextPropertyKey, value);
    }

    private static readonly DependencyPropertyKey DisplayTextPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(DisplayText),
            typeof(string),
            typeof(NNumberAnimation),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DisplayTextProperty = DisplayTextPropertyKey.DependencyProperty;

    public bool IsAnimating
    {
        get => (bool)GetValue(IsAnimatingProperty);
        private set => SetValue(IsAnimatingPropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsAnimatingPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsAnimating),
            typeof(bool),
            typeof(NNumberAnimation),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsAnimatingProperty = IsAnimatingPropertyKey.DependencyProperty;

    public event RoutedEventHandler Started
    {
        add => AddHandler(StartedEvent, value);
        remove => RemoveHandler(StartedEvent, value);
    }

    public static readonly RoutedEvent StartedEvent =
        ElementBase.RoutedEvent<NNumberAnimation, RoutedEventHandler>(nameof(StartedEvent));

    public event RoutedEventHandler Completed
    {
        add => AddHandler(CompletedEvent, value);
        remove => RemoveHandler(CompletedEvent, value);
    }

    public static readonly RoutedEvent CompletedEvent =
        ElementBase.RoutedEvent<NNumberAnimation, RoutedEventHandler>(nameof(CompletedEvent));

    public event RoutedPropertyChangedEventHandler<double> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>),
            typeof(NNumberAnimation));

    public void Play()
    {
        if (IsAnimating)
        {
            return;
        }

        StartAnimation(From, To);
    }

    public void Restart()
    {
        StopRendering();
        StartAnimation(From, To);
    }

    public void Stop()
    {
        StopRendering();
        IsAnimating = false;
    }

    private static void OnAnimationInputPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NNumberAnimation numberAnimation)
        {
            return;
        }

        if (!numberAnimation.RestartOnValueChanged && numberAnimation.hasLoaded)
        {
            return;
        }

        numberAnimation.RunActiveAnimation();
    }

    private static void OnActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NNumberAnimation numberAnimation)
        {
            numberAnimation.RunActiveAnimation();
        }
    }

    private static void OnFormatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NNumberAnimation numberAnimation)
        {
            numberAnimation.UpdateDisplayText();
        }
    }

    private static void OnCurrentValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NNumberAnimation numberAnimation)
        {
            return;
        }

        numberAnimation.UpdateDisplayText();
        numberAnimation.RaiseEvent(new RoutedPropertyChangedEventArgs<double>(
            (double)e.OldValue,
            (double)e.NewValue,
            ValueChangedEvent));
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        hasLoaded = true;
        RunActiveAnimation();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        StopRendering();
        IsAnimating = false;
        hasLoaded = false;
    }

    private void RunActiveAnimation()
    {
        if (!Active)
        {
            StopRendering();
            IsAnimating = false;
            if (!hasLoaded)
            {
                CurrentValue = From;
            }

            return;
        }

        if (!IsAnimationEnabled)
        {
            StopRendering();
            IsAnimating = false;
            CurrentValue = To;
            return;
        }

        if (!hasLoaded)
        {
            CurrentValue = From;
            return;
        }

        var start = UseCurrentValueAsFromOnChange ? CurrentValue : From;
        StopRendering();
        StartAnimation(start, To);
    }

    private void StartAnimation(double from, double to)
    {
        animationFrom = from;
        animationTo = to;
        lastRenderedValue = from;
        CurrentValue = from;

        if (!IsAnimationEnabled || Duration <= 0d || AreClose(from, to))
        {
            CurrentValue = to;
            IsAnimating = false;
            RaiseEvent(new RoutedEventArgs(CompletedEvent));
            return;
        }

        animationStartTime = DateTime.UtcNow;
        IsAnimating = true;
        RaiseEvent(new RoutedEventArgs(StartedEvent));
        SubscribeRendering();
    }

    private void SubscribeRendering()
    {
        if (isRenderingSubscribed)
        {
            return;
        }

        CompositionTarget.Rendering += HandleRendering;
        isRenderingSubscribed = true;
    }

    private void StopRendering()
    {
        if (!isRenderingSubscribed)
        {
            return;
        }

        CompositionTarget.Rendering -= HandleRendering;
        isRenderingSubscribed = false;
    }

    private void HandleRendering(object? sender, EventArgs e)
    {
        var elapsed = (DateTime.UtcNow - animationStartTime).TotalMilliseconds;
        var duration = Math.Max(1d, Duration);
        var progress = Math.Clamp(elapsed / duration, 0d, 1d);
        var easedProgress = EasingFunction?.Ease(progress) ?? EaseOutQuint(progress);
        var current = animationFrom + (animationTo - animationFrom) * easedProgress;

        if (progress >= 1d)
        {
            StopRendering();
            IsAnimating = false;
            CurrentValue = animationTo;
            RaiseEvent(new RoutedEventArgs(CompletedEvent));
            return;
        }

        if (!AreClose(lastRenderedValue, current))
        {
            lastRenderedValue = current;
            CurrentValue = current;
        }
    }

    private void UpdateDisplayText()
    {
        var formatted = FormatNumber(CurrentValue);
        var text = string.Concat(Prefix, formatted, Suffix);
        DisplayText = text;
        SetCurrentValue(TextProperty, text);
    }

    private string FormatNumber(double value)
    {
        var culture = ResolveCulture();
        var precision = Math.Max(0, Precision);

        if (!string.IsNullOrWhiteSpace(FormatString))
        {
            return value.ToString(FormatString, culture);
        }

        var rounded = Math.Round(value, precision, MidpointRounding.AwayFromZero);
        var format = ShowSeparator ? $"N{precision}" : $"F{precision}";
        return rounded.ToString(format, culture);
    }

    private CultureInfo ResolveCulture()
    {
        if (!string.IsNullOrWhiteSpace(Locale))
        {
            try
            {
                return CultureInfo.GetCultureInfo(Locale);
            }
            catch (CultureNotFoundException)
            {
                return CultureInfo.CurrentCulture;
            }
        }

        return CultureInfo.CurrentCulture;
    }

    private static double EaseOutQuint(double progress)
    {
        return 1d - Math.Pow(1d - progress, 5d);
    }

    private static bool AreClose(double left, double right)
    {
        return Math.Abs(left - right) < 0.0000001d;
    }
}
