using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NaiveUI.NControls.Themes;
using NaiveUI.NControls.Tools;
using System;

namespace NaiveUI.NControls.Controls;

public class NIcon : ContentControl
{
    private const double DefaultSize = 20d;

    static NIcon()
    {
        ElementBase.DefaultStyle<NIcon>(DefaultStyleKeyProperty);
    }

    public NIcon()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        UpdateResolvedState();
    }

    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly DependencyProperty DataProperty =
        ElementBase.Property<NIcon, Geometry?>(nameof(DataProperty), null, OnAppearancePropertyChanged);

    public Brush? IconBrush
    {
        get => (Brush?)GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    public static readonly DependencyProperty IconBrushProperty =
        ElementBase.Property<NIcon, Brush?>(nameof(IconBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? Color
    {
        get => (Brush?)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly DependencyProperty ColorProperty =
        ElementBase.Property<NIcon, Brush?>(nameof(ColorProperty), null, OnAppearancePropertyChanged);

    public object? Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        ElementBase.Property<NIcon, object?>(nameof(SizeProperty), null, OnAppearancePropertyChanged);

    public int Depth
    {
        get => (int)GetValue(DepthProperty);
        set => SetValue(DepthProperty, value);
    }

    public static readonly DependencyProperty DepthProperty =
        ElementBase.Property<NIcon, int>(nameof(DepthProperty), 0, OnAppearancePropertyChanged);

    public object? Component
    {
        get => GetValue(ComponentProperty);
        set => SetValue(ComponentProperty, value);
    }

    public static readonly DependencyProperty ComponentProperty =
        ElementBase.Property<NIcon, object?>(nameof(ComponentProperty), null, OnAppearancePropertyChanged);

    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public static readonly DependencyProperty StretchProperty =
        ElementBase.Property<NIcon, Stretch>(nameof(StretchProperty), Stretch.Uniform);

    public bool UseStroke
    {
        get => (bool)GetValue(UseStrokeProperty);
        set => SetValue(UseStrokeProperty, value);
    }

    public static readonly DependencyProperty UseStrokeProperty =
        ElementBase.Property<NIcon, bool>(nameof(UseStrokeProperty), false);

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public static readonly DependencyProperty StrokeThicknessProperty =
        ElementBase.Property<NIcon, double>(nameof(StrokeThicknessProperty), double.NaN, OnAppearancePropertyChanged);

    public object? ResolvedContent
    {
        get => GetValue(ResolvedContentProperty);
        private set => SetValue(ResolvedContentProperty, value);
    }

    public static readonly DependencyProperty ResolvedContentProperty =
        ElementBase.Property<NIcon, object?>(nameof(ResolvedContentProperty), null);

    public double ResolvedSize
    {
        get => (double)GetValue(ResolvedSizeProperty);
        private set => SetValue(ResolvedSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedSizeProperty =
        ElementBase.Property<NIcon, double>(nameof(ResolvedSizeProperty), DefaultSize);

    public Brush ResolvedBrush
    {
        get => (Brush)GetValue(ResolvedBrushProperty);
        private set => SetValue(ResolvedBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedBrushProperty =
        ElementBase.Property<NIcon, Brush>(nameof(ResolvedBrushProperty), Brushes.Black);

    public double ResolvedOpacity
    {
        get => (double)GetValue(ResolvedOpacityProperty);
        private set => SetValue(ResolvedOpacityProperty, value);
    }

    public static readonly DependencyProperty ResolvedOpacityProperty =
        ElementBase.Property<NIcon, double>(nameof(ResolvedOpacityProperty), 1d);

    public double ResolvedStrokeThickness
    {
        get => (double)GetValue(ResolvedStrokeThicknessProperty);
        private set => SetValue(ResolvedStrokeThicknessProperty, value);
    }

    public static readonly DependencyProperty ResolvedStrokeThicknessProperty =
        ElementBase.Property<NIcon, double>(nameof(ResolvedStrokeThicknessProperty), 1.75d);

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        UpdateResolvedContent();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == ForegroundProperty)
        {
            UpdateResolvedBrush();
        }
    }

    private static void OnAppearancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NIcon icon)
        {
            return;
        }

        icon.UpdateResolvedState(e.Property);
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
        UpdateResolvedOpacity();
        UpdateResolvedBrush();
    }

    private void UpdateResolvedState(DependencyProperty? property = null)
    {
        if (property is null || property == SizeProperty)
        {
            ResolvedSize = LengthValueParser.ResolveOrDefault(Size, DefaultSize);
        }

        if (property is null || property == ColorProperty || property == IconBrushProperty)
        {
            UpdateResolvedBrush();
        }

        if (property is null || property == DepthProperty)
        {
            UpdateResolvedOpacity();
        }

        if (property is null || property == ComponentProperty)
        {
            UpdateResolvedContent();
        }

        if (property is null || property == StrokeThicknessProperty || property == DataProperty)
        {
            UpdateResolvedStrokeThickness();
        }
    }

    private void UpdateResolvedContent()
    {
        ResolvedContent = Component ?? Content;
    }

    private void UpdateResolvedBrush()
    {
        ResolvedBrush = Color ?? IconBrush ?? Foreground ?? GetThemePrimaryBrush();
    }

    private void UpdateResolvedOpacity()
    {
        ResolvedOpacity = GetDepthOpacity(Depth);
    }

    private void UpdateResolvedStrokeThickness()
    {
        if (!double.IsNaN(StrokeThickness) && StrokeThickness > 0d)
        {
            ResolvedStrokeThickness = StrokeThickness;
            return;
        }

        var maxDimension = Data is null
            ? 0d
            : Math.Max(Data.Bounds.Width, Data.Bounds.Height);

        ResolvedStrokeThickness = maxDimension > 128d ? 32d : 1.75d;
    }

    private Brush GetThemePrimaryBrush()
    {
        if (TryFindResource("Theme.Text.Primary.Brush") is Brush brush)
        {
            return brush;
        }

        if (Application.Current?.TryFindResource("Theme.Text.Primary.Brush") is Brush appBrush)
        {
            return appBrush;
        }

        return ThemeManager.CurrentTheme == ThemeMode.Dark ? Brushes.White : Brushes.Black;
    }

    private static double GetDepthOpacity(int depth)
    {
        return ThemeManager.CurrentTheme switch
        {
            ThemeMode.Dark => depth switch
            {
                1 => 0.9d,
                2 => 0.82d,
                3 => 0.52d,
                4 => 0.38d,
                5 => 0.28d,
                _ => 1d
            },
            _ => depth switch
            {
                1 => 0.82d,
                2 => 0.72d,
                3 => 0.38d,
                4 => 0.24d,
                5 => 0.18d,
                _ => 1d
            }
        };
    }
}
