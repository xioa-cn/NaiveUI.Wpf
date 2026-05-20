using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NaiveUI.NControls.Themes;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NIconWrapper : ContentControl
{
    private const double DefaultBorderRadius = 6d;
    private const double DefaultSize = 24d;

    static NIconWrapper()
    {
        ElementBase.DefaultStyle<NIconWrapper>(DefaultStyleKeyProperty);
    }

    public NIconWrapper()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        UpdateResolvedState();
    }

    public object? Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        ElementBase.Property<NIconWrapper, object?>(nameof(SizeProperty), null, OnAppearancePropertyChanged);

    public double BorderRadius
    {
        get => (double)GetValue(BorderRadiusProperty);
        set => SetValue(BorderRadiusProperty, value);
    }

    public static readonly DependencyProperty BorderRadiusProperty =
        ElementBase.Property<NIconWrapper, double>(nameof(BorderRadiusProperty), DefaultBorderRadius, OnAppearancePropertyChanged);

    public Brush? Color
    {
        get => (Brush?)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly DependencyProperty ColorProperty =
        ElementBase.Property<NIconWrapper, Brush?>(nameof(ColorProperty), null);

    public Brush? IconColor
    {
        get => (Brush?)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public static readonly DependencyProperty IconColorProperty =
        ElementBase.Property<NIconWrapper, Brush?>(nameof(IconColorProperty), null, OnAppearancePropertyChanged);

    public double ResolvedSize
    {
        get => (double)GetValue(ResolvedSizeProperty);
        private set => SetValue(ResolvedSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedSizeProperty =
        ElementBase.Property<NIconWrapper, double>(nameof(ResolvedSizeProperty), DefaultSize);

    public CornerRadius ResolvedCornerRadius
    {
        get => (CornerRadius)GetValue(ResolvedCornerRadiusProperty);
        private set => SetValue(ResolvedCornerRadiusProperty, value);
    }

    public static readonly DependencyProperty ResolvedCornerRadiusProperty =
        ElementBase.Property<NIconWrapper, CornerRadius>(nameof(ResolvedCornerRadiusProperty), new CornerRadius(DefaultBorderRadius));

    public Brush ResolvedForeground
    {
        get => (Brush)GetValue(ResolvedForegroundProperty);
        private set => SetValue(ResolvedForegroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedForegroundProperty =
        ElementBase.Property<NIconWrapper, Brush>(nameof(ResolvedForegroundProperty), Brushes.White);

    private static void OnAppearancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NIconWrapper iconWrapper)
        {
            return;
        }

        iconWrapper.UpdateResolvedState(e.Property);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged += HandleThemeChanged;
        UpdateResolvedState();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged -= HandleThemeChanged;
    }

    private void HandleThemeChanged(ThemeMode mode)
    {
        UpdateResolvedForeground();
    }

    private void UpdateResolvedState(DependencyProperty? property = null)
    {
        if (property is null || property == SizeProperty)
        {
            ResolvedSize = LengthValueParser.ResolveOrDefault(Size, DefaultSize);
        }

        if (property is null || property == BorderRadiusProperty)
        {
            ResolvedCornerRadius = new CornerRadius(Math.Max(0d, BorderRadius));
        }

        if (property is null || property == IconColorProperty)
        {
            UpdateResolvedForeground();
        }
    }

    private void UpdateResolvedForeground()
    {
        ResolvedForeground = IconColor ?? GetThemeDefaultForeground();
    }

    private static Brush GetThemeDefaultForeground()
    {
        return ThemeManager.CurrentTheme == ThemeMode.Dark ? Brushes.Black : Brushes.White;
    }
}
