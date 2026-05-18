using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NaiveUI.NControls.Themes;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NButtonKind
{
    Filled,
    Secondary,
    Tertiary,
    Quaternary,
    Ghost,
    Dashed,
    Text
}

public enum NControlSize
{
    Tiny,
    Small,
    Medium,
    Large
}

public class NButton : Button
{
    private static readonly CornerRadius DefaultCornerRadius = new(3);
    private static readonly CornerRadius RoundCornerRadius = new(999);

    static NButton()
    {
        ElementBase.DefaultStyle<NButton>(DefaultStyleKeyProperty);
    }

    public NButton()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsMouseOverProperty || e.Property == IsPressedProperty || e.Property == IsEnabledProperty)
        {
            UpdateVisualState();
        }
    }

    public Grade Grade
    {
        get => (Grade)GetValue(GradeProperty);
        set => SetValue(GradeProperty, value);
    }

    public static readonly DependencyProperty GradeProperty =
        ElementBase.Property<NButton, Grade>(nameof(GradeProperty), Grade.Default, OnAppearanceChanged);

    public NButtonKind Kind
    {
        get => (NButtonKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public static readonly DependencyProperty KindProperty =
        ElementBase.Property<NButton, NButtonKind>(nameof(KindProperty), NButtonKind.Filled, OnAppearanceChanged);

    public NControlSize Size
    {
        get => (NControlSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        ElementBase.Property<NButton, NControlSize>(nameof(SizeProperty), NControlSize.Medium);

    public bool Round
    {
        get => (bool)GetValue(RoundProperty);
        set => SetValue(RoundProperty, value);
    }

    public static readonly DependencyProperty RoundProperty =
        ElementBase.Property<NButton, bool>(nameof(RoundProperty), false, OnCornerAppearanceChanged);

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        ElementBase.Property<NButton, CornerRadius>(nameof(CornerRadiusProperty), DefaultCornerRadius, OnCornerAppearanceChanged);

    public CornerRadius ResolvedCornerRadius
    {
        get => (CornerRadius)GetValue(ResolvedCornerRadiusProperty);
        private set => SetValue(ResolvedCornerRadiusProperty, value);
    }

    public static readonly DependencyProperty ResolvedCornerRadiusProperty =
        ElementBase.Property<NButton, CornerRadius>(nameof(ResolvedCornerRadiusProperty), DefaultCornerRadius);

    public double ResolvedCornerRadiusValue
    {
        get => (double)GetValue(ResolvedCornerRadiusValueProperty);
        private set => SetValue(ResolvedCornerRadiusValueProperty, value);
    }

    public static readonly DependencyProperty ResolvedCornerRadiusValueProperty =
        ElementBase.Property<NButton, double>(nameof(ResolvedCornerRadiusValueProperty), 3d);

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public static readonly DependencyProperty IsLoadingProperty =
        ElementBase.Property<NButton, bool>(nameof(IsLoadingProperty), false, OnLoadingChanged);

    public Brush? LoadingBrush
    {
        get => (Brush?)GetValue(LoadingBrushProperty);
        set => SetValue(LoadingBrushProperty, value);
    }

    public static readonly DependencyProperty LoadingBrushProperty =
        ElementBase.Property<NButton, Brush?>(nameof(LoadingBrushProperty), null, OnLoadingBrushChanged);

    public Brush? CustomBackgroundBrush
    {
        get => (Brush?)GetValue(CustomBackgroundBrushProperty);
        set => SetValue(CustomBackgroundBrushProperty, value);
    }

    public static readonly DependencyProperty CustomBackgroundBrushProperty =
        ElementBase.Property<NButton, Brush?>(nameof(CustomBackgroundBrushProperty), null, OnAppearanceChanged);

    public Brush? CustomBorderBrush
    {
        get => (Brush?)GetValue(CustomBorderBrushProperty);
        set => SetValue(CustomBorderBrushProperty, value);
    }

    public static readonly DependencyProperty CustomBorderBrushProperty =
        ElementBase.Property<NButton, Brush?>(nameof(CustomBorderBrushProperty), null, OnAppearanceChanged);

    public Brush? CustomForegroundBrush
    {
        get => (Brush?)GetValue(CustomForegroundBrushProperty);
        set => SetValue(CustomForegroundBrushProperty, value);
    }

    public static readonly DependencyProperty CustomForegroundBrushProperty =
        ElementBase.Property<NButton, Brush?>(nameof(CustomForegroundBrushProperty), null, OnAppearanceChanged);

    public Brush? CustomHoverBackgroundBrush
    {
        get => (Brush?)GetValue(CustomHoverBackgroundBrushProperty);
        set => SetValue(CustomHoverBackgroundBrushProperty, value);
    }

    public static readonly DependencyProperty CustomHoverBackgroundBrushProperty =
        ElementBase.Property<NButton, Brush?>(nameof(CustomHoverBackgroundBrushProperty), null, OnAppearanceChanged);

    public Brush? CustomHoverBorderBrush
    {
        get => (Brush?)GetValue(CustomHoverBorderBrushProperty);
        set => SetValue(CustomHoverBorderBrushProperty, value);
    }

    public static readonly DependencyProperty CustomHoverBorderBrushProperty =
        ElementBase.Property<NButton, Brush?>(nameof(CustomHoverBorderBrushProperty), null, OnAppearanceChanged);

    public Brush? CustomHoverForegroundBrush
    {
        get => (Brush?)GetValue(CustomHoverForegroundBrushProperty);
        set => SetValue(CustomHoverForegroundBrushProperty, value);
    }

    public static readonly DependencyProperty CustomHoverForegroundBrushProperty =
        ElementBase.Property<NButton, Brush?>(nameof(CustomHoverForegroundBrushProperty), null, OnAppearanceChanged);

    public Brush? CustomPressedBackgroundBrush
    {
        get => (Brush?)GetValue(CustomPressedBackgroundBrushProperty);
        set => SetValue(CustomPressedBackgroundBrushProperty, value);
    }

    public static readonly DependencyProperty CustomPressedBackgroundBrushProperty =
        ElementBase.Property<NButton, Brush?>(nameof(CustomPressedBackgroundBrushProperty), null, OnAppearanceChanged);

    public Brush? CustomPressedBorderBrush
    {
        get => (Brush?)GetValue(CustomPressedBorderBrushProperty);
        set => SetValue(CustomPressedBorderBrushProperty, value);
    }

    public static readonly DependencyProperty CustomPressedBorderBrushProperty =
        ElementBase.Property<NButton, Brush?>(nameof(CustomPressedBorderBrushProperty), null, OnAppearanceChanged);

    public Brush? CustomPressedForegroundBrush
    {
        get => (Brush?)GetValue(CustomPressedForegroundBrushProperty);
        set => SetValue(CustomPressedForegroundBrushProperty, value);
    }

    public static readonly DependencyProperty CustomPressedForegroundBrushProperty =
        ElementBase.Property<NButton, Brush?>(nameof(CustomPressedForegroundBrushProperty), null, OnAppearanceChanged);

    public Brush ResolvedBackground
    {
        get => (Brush)GetValue(ResolvedBackgroundProperty);
        private set => SetValue(ResolvedBackgroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedBackgroundProperty =
        ElementBase.Property<NButton, Brush>(nameof(ResolvedBackgroundProperty), Brushes.Transparent);

    public Brush ResolvedBorderBrush
    {
        get => (Brush)GetValue(ResolvedBorderBrushProperty);
        private set => SetValue(ResolvedBorderBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedBorderBrushProperty =
        ElementBase.Property<NButton, Brush>(nameof(ResolvedBorderBrushProperty), Brushes.Transparent);

    public Brush ResolvedForeground
    {
        get => (Brush)GetValue(ResolvedForegroundProperty);
        private set => SetValue(ResolvedForegroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedForegroundProperty =
        ElementBase.Property<NButton, Brush>(nameof(ResolvedForegroundProperty), Brushes.Black);

    public Brush ResolvedHoverBorderBrush
    {
        get => (Brush)GetValue(ResolvedHoverBorderBrushProperty);
        private set => SetValue(ResolvedHoverBorderBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedHoverBorderBrushProperty =
        ElementBase.Property<NButton, Brush>(nameof(ResolvedHoverBorderBrushProperty), Brushes.Transparent);

    public Brush ResolvedPressedBorderBrush
    {
        get => (Brush)GetValue(ResolvedPressedBorderBrushProperty);
        private set => SetValue(ResolvedPressedBorderBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedPressedBorderBrushProperty =
        ElementBase.Property<NButton, Brush>(nameof(ResolvedPressedBorderBrushProperty), Brushes.Transparent);

    public Brush ResolvedHoverForeground
    {
        get => (Brush)GetValue(ResolvedHoverForegroundProperty);
        private set => SetValue(ResolvedHoverForegroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedHoverForegroundProperty =
        ElementBase.Property<NButton, Brush>(nameof(ResolvedHoverForegroundProperty), Brushes.Black);

    public Brush ResolvedPressedForeground
    {
        get => (Brush)GetValue(ResolvedPressedForegroundProperty);
        private set => SetValue(ResolvedPressedForegroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedPressedForegroundProperty =
        ElementBase.Property<NButton, Brush>(nameof(ResolvedPressedForegroundProperty), Brushes.Black);

    public Brush ResolvedHoverBackground
    {
        get => (Brush)GetValue(ResolvedHoverBackgroundProperty);
        private set => SetValue(ResolvedHoverBackgroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedHoverBackgroundProperty =
        ElementBase.Property<NButton, Brush>(nameof(ResolvedHoverBackgroundProperty), Brushes.Transparent);

    public Brush ResolvedPressedBackground
    {
        get => (Brush)GetValue(ResolvedPressedBackgroundProperty);
        private set => SetValue(ResolvedPressedBackgroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedPressedBackgroundProperty =
        ElementBase.Property<NButton, Brush>(nameof(ResolvedPressedBackgroundProperty), Brushes.Transparent);

    public Brush ResolvedLoadingBrush
    {
        get => (Brush)GetValue(ResolvedLoadingBrushProperty);
        private set => SetValue(ResolvedLoadingBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedLoadingBrushProperty =
        ElementBase.Property<NButton, Brush>(nameof(ResolvedLoadingBrushProperty), Brushes.Black);

    private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NButton button)
        {
            button.UpdateResolvedBrushes();
        }
    }

    private static void OnCornerAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NButton button)
        {
            button.UpdateResolvedCornerRadius();
        }
    }

    private static void OnLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NButton button)
        {
            button.ApplyLoadingState();
        }
    }

    private static void OnLoadingBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NButton button)
        {
            button.UpdateVisualState();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged += HandleThemeChanged;
        UpdateResolvedCornerRadius();
        UpdateResolvedBrushes();
        ApplyLoadingState();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged -= HandleThemeChanged;
    }

    private void HandleThemeChanged(ThemeMode mode)
    {
        UpdateResolvedBrushes();
    }

    private void UpdateResolvedBrushes()
    {
        if (HasCustomAppearance())
        {
            SetResolved(
                CustomBackgroundBrush ?? GetBrush("Theme.Surface.0.Brush"),
                CustomBorderBrush ?? CustomBackgroundBrush ?? GetBrush("Theme.Border.Strong.Brush"),
                CustomForegroundBrush ?? GetBrush("Theme.Text.Primary.Brush"),
                CustomHoverBackgroundBrush ?? CustomBackgroundBrush ?? GetBrush("Theme.Fill.Hover.Brush"),
                CustomHoverBorderBrush ?? CustomBorderBrush ?? CustomBackgroundBrush ?? GetBrush("Theme.Border.Strong.Brush"),
                CustomHoverForegroundBrush ?? CustomForegroundBrush ?? GetBrush("Theme.Text.Primary.Brush"),
                CustomPressedBackgroundBrush ?? CustomHoverBackgroundBrush ?? CustomBackgroundBrush ?? GetBrush("Theme.Fill.2.Brush"),
                CustomPressedBorderBrush ?? CustomHoverBorderBrush ?? CustomBorderBrush ?? CustomBackgroundBrush ?? GetBrush("Theme.Border.Strong.Brush"),
                CustomPressedForegroundBrush ?? CustomHoverForegroundBrush ?? CustomForegroundBrush ?? GetBrush("Theme.Text.Primary.Brush"));
            UpdateVisualState();
            return;
        }

        var tonePrefix = Grade switch
        {
            Grade.Primary => "Primary",
            Grade.Info => "Info",
            Grade.Success => "Success",
            Grade.Warning => "Warning",
            Grade.Error => "Error",
            _ => "Default"
        };

        if (Kind == NButtonKind.Filled)
        {
            if (Grade == Grade.Default)
            {
                SetResolved(
                    "Theme.Surface.0.Brush",
                    "Theme.Border.Strong.Brush",
                    "Theme.Text.Primary.Brush",
                    "Theme.Surface.0.Brush",
                    "Primary.Hover.Brush",
                    "Primary.Hover.Brush",
                    "Theme.Surface.0.Brush",
                    "Primary.Pressed.Brush",
                    "Primary.Pressed.Brush");
            }
            else
            {
                SetResolved(
                    $"{tonePrefix}.First.Brush",
                    $"{tonePrefix}.First.Brush",
                    "Theme.Text.Inverse.Brush",
                    $"{tonePrefix}.Hover.Brush",
                    $"{tonePrefix}.Hover.Brush",
                    "Theme.Text.Inverse.Brush",
                    $"{tonePrefix}.Pressed.Brush",
                    $"{tonePrefix}.Pressed.Brush",
                    "Theme.Text.Inverse.Brush");
            }
        }
        else if (Kind == NButtonKind.Secondary)
        {
            if (Grade == Grade.Default)
            {
                SetResolved(
                    "Theme.Fill.2.Brush",
                    "Theme.Fill.2.Brush",
                    "Theme.Text.Primary.Brush",
                    "Theme.Fill.2Hover.Brush",
                    "Theme.Fill.2Hover.Brush",
                    "Theme.Text.Primary.Brush",
                    "Theme.Fill.2Pressed.Brush",
                    "Theme.Fill.2Pressed.Brush",
                    "Theme.Text.Primary.Brush");
            }
            else
            {
                SetResolved(
                    $"{tonePrefix}.Third.Brush",
                    $"{tonePrefix}.Third.Brush",
                    $"{tonePrefix}.First.Brush",
                    $"{tonePrefix}.Third.Brush",
                    $"{tonePrefix}.Hover.Brush",
                    $"{tonePrefix}.Hover.Brush",
                    $"{tonePrefix}.Third.Brush",
                    $"{tonePrefix}.Pressed.Brush",
                    $"{tonePrefix}.Pressed.Brush");
            }
        }
        else if (Kind == NButtonKind.Tertiary)
        {
            if (Grade == Grade.Default)
            {
                SetResolved(
                    "Theme.Surface.0.Brush",
                    "Theme.Border.Brush",
                    "Theme.Text.Secondary.Brush",
                    "Theme.Fill.Hover.Brush",
                    "Primary.Hover.Brush",
                    "Primary.Hover.Brush",
                    "Theme.Fill.2.Brush",
                    "Primary.Pressed.Brush",
                    "Primary.Pressed.Brush");
            }
            else
            {
                SetResolved(
                    "Theme.Surface.0.Brush",
                    $"{tonePrefix}.First.Brush",
                    $"{tonePrefix}.First.Brush",
                    $"{tonePrefix}.Third.Brush",
                    $"{tonePrefix}.Hover.Brush",
                    $"{tonePrefix}.Hover.Brush",
                    $"{tonePrefix}.Third.Brush",
                    $"{tonePrefix}.Pressed.Brush",
                    $"{tonePrefix}.Pressed.Brush");
            }
        }
        else if (Kind == NButtonKind.Quaternary)
        {
            if (Grade == Grade.Default)
            {
                SetResolved(
                    "Transparent",
                    "Transparent",
                    "Theme.Text.Secondary.Brush",
                    "Theme.Fill.Hover.Brush",
                    "Transparent",
                    "Theme.Text.Primary.Brush",
                    "Theme.Fill.2.Brush",
                    "Transparent",
                    "Theme.Text.Primary.Brush");
            }
            else
            {
                SetResolved(
                    "Transparent",
                    "Transparent",
                    $"{tonePrefix}.First.Brush",
                    $"{tonePrefix}.Third.Brush",
                    "Transparent",
                    $"{tonePrefix}.Hover.Brush",
                    $"{tonePrefix}.Third.Brush",
                    "Transparent",
                    $"{tonePrefix}.Pressed.Brush");
            }
        }
        else if (Kind == NButtonKind.Ghost)
        {
            if (Grade == Grade.Default)
            {
                SetResolved(
                    "Transparent",
                    "Theme.Border.Strong.Brush",
                    "Theme.Text.Secondary.Brush",
                    "Theme.Fill.Hover.Brush",
                    "Primary.Hover.Brush",
                    "Primary.Hover.Brush",
                    "Theme.Fill.2.Brush",
                    "Primary.Pressed.Brush",
                    "Primary.Pressed.Brush");
            }
            else
            {
                SetResolved(
                    "Transparent",
                    $"{tonePrefix}.First.Brush",
                    $"{tonePrefix}.First.Brush",
                    $"{tonePrefix}.Third.Brush",
                    $"{tonePrefix}.Hover.Brush",
                    $"{tonePrefix}.Hover.Brush",
                    $"{tonePrefix}.Third.Brush",
                    $"{tonePrefix}.Pressed.Brush",
                    $"{tonePrefix}.Pressed.Brush");
            }
        }
        else if (Kind == NButtonKind.Dashed)
        {
            if (Grade == Grade.Default)
            {
                SetResolved(
                    "Transparent",
                    "Theme.Border.Strong.Brush",
                    "Theme.Text.Secondary.Brush",
                    "Theme.Fill.Hover.Brush",
                    "Primary.Hover.Brush",
                    "Primary.Hover.Brush",
                    "Theme.Fill.2.Brush",
                    "Primary.Pressed.Brush",
                    "Primary.Pressed.Brush");
            }
            else
            {
                SetResolved(
                    "Transparent",
                    $"{tonePrefix}.First.Brush",
                    $"{tonePrefix}.First.Brush",
                    $"{tonePrefix}.Third.Brush",
                    $"{tonePrefix}.Hover.Brush",
                    $"{tonePrefix}.Hover.Brush",
                    $"{tonePrefix}.Third.Brush",
                    $"{tonePrefix}.Pressed.Brush",
                    $"{tonePrefix}.Pressed.Brush");
            }
        }
        else if (Grade == Grade.Default)
        {
            SetResolved(
                "Transparent",
                "Transparent",
                "Theme.Text.Secondary.Brush",
                "Theme.Fill.Hover.Brush",
                "Transparent",
                "Primary.Hover.Brush",
                "Theme.Fill.2.Brush",
                "Transparent",
                "Primary.Pressed.Brush");
        }
        else
        {
            SetResolved(
                "Transparent",
                "Transparent",
                $"{tonePrefix}.First.Brush",
                $"{tonePrefix}.Third.Brush",
                "Transparent",
                $"{tonePrefix}.Hover.Brush",
                $"{tonePrefix}.Third.Brush",
                "Transparent",
                $"{tonePrefix}.Pressed.Brush");
        }

        UpdateVisualState();
    }

    private void UpdateResolvedCornerRadius()
    {
        var resolved = Round && CornerRadius.Equals(DefaultCornerRadius)
            ? RoundCornerRadius
            : CornerRadius;

        ResolvedCornerRadius = resolved;
        ResolvedCornerRadiusValue = resolved.TopLeft;
    }

    private bool HasCustomAppearance()
    {
        return Grade == Grade.Customize
               || CustomBackgroundBrush is not null
               || CustomBorderBrush is not null
               || CustomForegroundBrush is not null
               || CustomHoverBackgroundBrush is not null
               || CustomHoverBorderBrush is not null
               || CustomHoverForegroundBrush is not null
               || CustomPressedBackgroundBrush is not null
               || CustomPressedBorderBrush is not null
               || CustomPressedForegroundBrush is not null;
    }

    private void ApplyLoadingState()
    {
        UpdateVisualState();
    }

    private void SetResolved(
        string backgroundKey,
        string borderKey,
        string foregroundKey,
        string hoverBackgroundKey,
        string hoverBorderKey,
        string hoverForegroundKey,
        string pressedBackgroundKey,
        string pressedBorderKey,
        string pressedForegroundKey)
    {
        SetResolved(
            GetBrush(backgroundKey),
            GetBrush(borderKey),
            GetBrush(foregroundKey),
            GetBrush(hoverBackgroundKey),
            GetBrush(hoverBorderKey),
            GetBrush(hoverForegroundKey),
            GetBrush(pressedBackgroundKey),
            GetBrush(pressedBorderKey),
            GetBrush(pressedForegroundKey));
    }

    private void SetResolved(
        Brush background,
        Brush border,
        Brush foreground,
        Brush hoverBackground,
        Brush hoverBorder,
        Brush hoverForeground,
        Brush pressedBackground,
        Brush pressedBorder,
        Brush pressedForeground)
    {
        ResolvedBackground = background;
        ResolvedBorderBrush = border;
        ResolvedForeground = foreground;
        ResolvedHoverBackground = hoverBackground;
        ResolvedHoverBorderBrush = hoverBorder;
        ResolvedHoverForeground = hoverForeground;
        ResolvedPressedBackground = pressedBackground;
        ResolvedPressedBorderBrush = pressedBorder;
        ResolvedPressedForeground = pressedForeground;
    }

    private void UpdateVisualState()
    {
        if (!IsEnabled)
        {
            Background = ResolvedBackground;
            BorderBrush = ResolvedBorderBrush;
            Foreground = ResolvedForeground;
            ResolvedLoadingBrush = LoadingBrush ?? ResolvedForeground;
            return;
        }

        if (IsPressed)
        {
            Background = ResolvedPressedBackground;
            BorderBrush = ResolvedPressedBorderBrush;
            Foreground = ResolvedPressedForeground;
            ResolvedLoadingBrush = LoadingBrush ?? ResolvedPressedForeground;
            return;
        }

        if (IsMouseOver)
        {
            Background = ResolvedHoverBackground;
            BorderBrush = ResolvedHoverBorderBrush;
            Foreground = ResolvedHoverForeground;
            ResolvedLoadingBrush = LoadingBrush ?? ResolvedHoverForeground;
            return;
        }

        Background = ResolvedBackground;
        BorderBrush = ResolvedBorderBrush;
        Foreground = ResolvedForeground;
        ResolvedLoadingBrush = LoadingBrush ?? ResolvedForeground;
    }

    private static Brush GetBrush(string key)
    {
        if (string.Equals(key, "Transparent", StringComparison.Ordinal))
        {
            return Brushes.Transparent;
        }

        if (Application.Current?.TryFindResource(key) is Brush brush)
        {
            return brush;
        }

        return Brushes.Transparent;
    }
}
