using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NaiveUI.NControls.Themes;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NBadgeType
{
    Default,
    Info,
    Success,
    Warning,
    Error
}

public class NBadge : ContentControl
{
    private const string BadgeSurfacePartName = "PART_BadgeSurface";
    private static readonly Thickness DefaultPadding = new(6d, 0d, 6d, 0d);
    private FrameworkElement? badgeSurfacePart;

    static NBadge()
    {
        ElementBase.DefaultStyle<NBadge>(DefaultStyleKeyProperty);
    }

    public NBadge()
    {
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        SizeChanged += HandleSizeChanged;

        UpdateResolvedState();
    }

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        ElementBase.Property<NBadge, object?>(nameof(ValueProperty), null, OnBadgeStatePropertyChanged);

    public int? Max
    {
        get => (int?)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    public static readonly DependencyProperty MaxProperty =
        DependencyProperty.Register(
            nameof(Max),
            typeof(int?),
            typeof(NBadge),
            new PropertyMetadata(null, OnBadgeStatePropertyChanged));

    public bool Dot
    {
        get => (bool)GetValue(DotProperty);
        set => SetValue(DotProperty, value);
    }

    public static readonly DependencyProperty DotProperty =
        ElementBase.Property<NBadge, bool>(nameof(DotProperty), false, OnBadgeStatePropertyChanged);

    public NBadgeType Type
    {
        get => (NBadgeType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public static readonly DependencyProperty TypeProperty =
        ElementBase.Property<NBadge, NBadgeType>(nameof(TypeProperty), NBadgeType.Default, OnBadgeVisualPropertyChanged);

    public bool Show
    {
        get => (bool)GetValue(ShowProperty);
        set => SetValue(ShowProperty, value);
    }

    public static readonly DependencyProperty ShowProperty =
        ElementBase.Property<NBadge, bool>(nameof(ShowProperty), true, OnBadgeStatePropertyChanged);

    public bool ShowZero
    {
        get => (bool)GetValue(ShowZeroProperty);
        set => SetValue(ShowZeroProperty, value);
    }

    public static readonly DependencyProperty ShowZeroProperty =
        ElementBase.Property<NBadge, bool>(nameof(ShowZeroProperty), false, OnBadgeStatePropertyChanged);

    public bool Processing
    {
        get => (bool)GetValue(ProcessingProperty);
        set => SetValue(ProcessingProperty, value);
    }

    public static readonly DependencyProperty ProcessingProperty =
        ElementBase.Property<NBadge, bool>(nameof(ProcessingProperty), false);

    public Brush? Color
    {
        get => (Brush?)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly DependencyProperty ColorProperty =
        ElementBase.Property<NBadge, Brush?>(nameof(ColorProperty), null, OnBadgeVisualPropertyChanged);

    public Point Offset
    {
        get => (Point)GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    public static readonly DependencyProperty OffsetProperty =
        ElementBase.Property<NBadge, Point>(nameof(OffsetProperty), new Point(0d, 0d), OnBadgeStatePropertyChanged);

    public bool HasHostContent
    {
        get => (bool)GetValue(HasHostContentProperty);
        private set => SetValue(HasHostContentProperty, value);
    }

    public static readonly DependencyProperty HasHostContentProperty =
        ElementBase.Property<NBadge, bool>(nameof(HasHostContentProperty), false);

    public bool ShowBadge
    {
        get => (bool)GetValue(ShowBadgeProperty);
        private set => SetValue(ShowBadgeProperty, value);
    }

    public static readonly DependencyProperty ShowBadgeProperty =
        ElementBase.Property<NBadge, bool>(nameof(ShowBadgeProperty), false);

    public bool UseTextDisplay
    {
        get => (bool)GetValue(UseTextDisplayProperty);
        private set => SetValue(UseTextDisplayProperty, value);
    }

    public static readonly DependencyProperty UseTextDisplayProperty =
        ElementBase.Property<NBadge, bool>(nameof(UseTextDisplayProperty), true);

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        private set => SetValue(DisplayTextProperty, value);
    }

    public static readonly DependencyProperty DisplayTextProperty =
        ElementBase.Property<NBadge, string>(nameof(DisplayTextProperty), string.Empty);

    public object? DisplayValueContent
    {
        get => GetValue(DisplayValueContentProperty);
        private set => SetValue(DisplayValueContentProperty, value);
    }

    public static readonly DependencyProperty DisplayValueContentProperty =
        ElementBase.Property<NBadge, object?>(nameof(DisplayValueContentProperty), null);

    public double ResolvedHeight
    {
        get => (double)GetValue(ResolvedHeightProperty);
        private set => SetValue(ResolvedHeightProperty, value);
    }

    public static readonly DependencyProperty ResolvedHeightProperty =
        ElementBase.Property<NBadge, double>(nameof(ResolvedHeightProperty), 18d);

    public double ResolvedMinWidth
    {
        get => (double)GetValue(ResolvedMinWidthProperty);
        private set => SetValue(ResolvedMinWidthProperty, value);
    }

    public static readonly DependencyProperty ResolvedMinWidthProperty =
        ElementBase.Property<NBadge, double>(nameof(ResolvedMinWidthProperty), 18d);

    public double ResolvedDotSize
    {
        get => (double)GetValue(ResolvedDotSizeProperty);
        private set => SetValue(ResolvedDotSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedDotSizeProperty =
        ElementBase.Property<NBadge, double>(nameof(ResolvedDotSizeProperty), 8d);

    public CornerRadius ResolvedCornerRadius
    {
        get => (CornerRadius)GetValue(ResolvedCornerRadiusProperty);
        private set => SetValue(ResolvedCornerRadiusProperty, value);
    }

    public static readonly DependencyProperty ResolvedCornerRadiusProperty =
        ElementBase.Property<NBadge, CornerRadius>(nameof(ResolvedCornerRadiusProperty), new CornerRadius(9d));

    public CornerRadius ResolvedDotCornerRadius
    {
        get => (CornerRadius)GetValue(ResolvedDotCornerRadiusProperty);
        private set => SetValue(ResolvedDotCornerRadiusProperty, value);
    }

    public static readonly DependencyProperty ResolvedDotCornerRadiusProperty =
        ElementBase.Property<NBadge, CornerRadius>(nameof(ResolvedDotCornerRadiusProperty), new CornerRadius(4d));

    public Thickness ResolvedBadgeMargin
    {
        get => (Thickness)GetValue(ResolvedBadgeMarginProperty);
        private set => SetValue(ResolvedBadgeMarginProperty, value);
    }

    public static readonly DependencyProperty ResolvedBadgeMarginProperty =
        ElementBase.Property<NBadge, Thickness>(nameof(ResolvedBadgeMarginProperty), new Thickness(0d));

    public Brush ResolvedBackground
    {
        get => (Brush)GetValue(ResolvedBackgroundProperty);
        private set => SetValue(ResolvedBackgroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedBackgroundProperty =
        ElementBase.Property<NBadge, Brush>(nameof(ResolvedBackgroundProperty), Brushes.Transparent);

    public override void OnApplyTemplate()
    {
        if (badgeSurfacePart is not null)
        {
            badgeSurfacePart.SizeChanged -= HandleBadgeSurfaceSizeChanged;
        }

        base.OnApplyTemplate();

        badgeSurfacePart = GetTemplateChild(BadgeSurfacePartName) as FrameworkElement;
        if (badgeSurfacePart is not null)
        {
            badgeSurfacePart.SizeChanged += HandleBadgeSurfaceSizeChanged;
        }

        UpdateBadgePosition();
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        UpdateResolvedState();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == FontSizeProperty
            || e.Property == FontFamilyProperty
            || e.Property == FontStyleProperty
            || e.Property == FontStretchProperty
            || e.Property == FontWeightProperty
            || e.Property == PaddingProperty
            || e.Property == ForegroundProperty)
        {
            UpdateResolvedState();
        }
    }

    private static void OnBadgeStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NBadge badge)
        {
            badge.UpdateResolvedState();
        }
    }

    private static void OnBadgeVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NBadge badge)
        {
            badge.UpdateVisualState();
        }
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged += HandleThemeChanged;
        UpdateResolvedState();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged -= HandleThemeChanged;
    }

    private void HandleThemeChanged(ThemeMode mode)
    {
        UpdateVisualState();
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateBadgePosition();
    }

    private void HandleBadgeSurfaceSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateBadgePosition();
    }

    private void UpdateResolvedState()
    {
        UpdateHostState();
        UpdateDisplayState();
        UpdateMetrics();
        UpdateVisualState();
        UpdateBadgePosition();
    }

    private void UpdateHostState()
    {
        HasHostContent = HasMeaningfulContent(Content);
    }

    private void UpdateDisplayState()
    {
        if (!TryResolveDisplay(out var displayText, out var displayContent, out var useTextDisplay))
        {
            ShowBadge = Show && Dot;
            DisplayText = string.Empty;
            DisplayValueContent = null;
            UseTextDisplay = true;
            return;
        }

        ShowBadge = Show;
        DisplayText = displayText;
        DisplayValueContent = displayContent;
        UseTextDisplay = useTextDisplay;
    }

    private void UpdateMetrics()
    {
        var padding = ResolvePadding();
        var dotSize = 8d;
        var height = Math.Max(18d, Math.Round(FontSize + padding.Top + padding.Bottom + 4d, 0));

        ResolvedDotSize = dotSize;
        ResolvedDotCornerRadius = new CornerRadius(dotSize / 2d);
        ResolvedHeight = height;
        ResolvedMinWidth = height;
        ResolvedCornerRadius = new CornerRadius(height / 2d);
    }

    private void UpdateVisualState()
    {
        if (Color is not null)
        {
            ResolvedBackground = Color;
            return;
        }

        var resourceKey = Type switch
        {
            NBadgeType.Info => "Info.First.Color",
            NBadgeType.Success => "Success.First.Color",
            NBadgeType.Warning => "Warning.First.Color",
            NBadgeType.Error => "Error.First.Color",
            _ => "Error.First.Color"
        };

        ResolvedBackground = CreateBrush(GetResourceColor(resourceKey, Colors.Red));
    }

    private void UpdateBadgePosition()
    {
        var offsetX = Offset.X;
        var offsetY = Offset.Y;

        if (!HasHostContent)
        {
            ResolvedBadgeMargin = new Thickness(offsetX, offsetY, 0d, 0d);
            return;
        }

        var (badgeWidth, badgeHeight) = GetCurrentBadgeSize();
        ResolvedBadgeMargin = new Thickness(
            0d,
            -badgeHeight / 2d + offsetY,
            -badgeWidth / 2d - offsetX,
            0d);
    }

    private bool TryResolveDisplay(out string displayText, out object? displayContent, out bool useTextDisplay)
    {
        displayText = string.Empty;
        displayContent = null;
        useTextDisplay = true;

        if (Dot)
        {
            return false;
        }

        if (Value is null)
        {
            return false;
        }

        if (Value is string text)
        {
            var trimmed = text.Trim();
            if (trimmed.Length == 0)
            {
                return false;
            }

            if (TryGetNumericValue(trimmed, out var numericValue))
            {
                if (!ShowZero && numericValue == 0d)
                {
                    return false;
                }

                displayText = ApplyOverflow(trimmed, numericValue);
                return true;
            }

            displayText = trimmed;
            return true;
        }

        if (TryGetNumericValue(Value, out var numberValue))
        {
            if (!ShowZero && numberValue == 0d)
            {
                return false;
            }

            displayText = ApplyOverflow(FormatValue(Value), numberValue);
            return true;
        }

        displayContent = Value;
        useTextDisplay = false;
        return true;
    }

    private (double Width, double Height) GetCurrentBadgeSize()
    {
        if (Dot)
        {
            return (ResolvedDotSize, ResolvedDotSize);
        }

        if (badgeSurfacePart is not null
            && badgeSurfacePart.ActualWidth > 0d
            && badgeSurfacePart.ActualHeight > 0d)
        {
            return (badgeSurfacePart.ActualWidth, badgeSurfacePart.ActualHeight);
        }

        var height = ResolvedHeight;
        if (!UseTextDisplay)
        {
            return (ResolvedMinWidth, height);
        }

        var padding = ResolvePadding();
        var textWidth = MeasureTextWidth(DisplayText);
        var width = Math.Max(ResolvedMinWidth, textWidth + padding.Left + padding.Right);
        return (width, height);
    }

    private Thickness ResolvePadding()
    {
        var valueSource = DependencyPropertyHelper.GetValueSource(this, PaddingProperty);
        return valueSource.BaseValueSource == BaseValueSource.Default ? DefaultPadding : Padding;
    }

    private double MeasureTextWidth(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0d;
        }

        var dpi = VisualTreeHelper.GetDpi(this);
        var formattedText = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
            FontSize,
            Brushes.Black,
            dpi.PixelsPerDip);

        return Math.Ceiling(formattedText.Width);
    }

    private string ApplyOverflow(string originalText, double numericValue)
    {
        if (Max is int max && numericValue > max)
        {
            return $"{max}+";
        }

        return originalText;
    }

    private static bool HasMeaningfulContent(object? content)
    {
        return content switch
        {
            null => false,
            string text => !string.IsNullOrWhiteSpace(text),
            _ => true
        };
    }

    private static bool TryGetNumericValue(object value, out double numericValue)
    {
        switch (value)
        {
            case byte byteValue:
                numericValue = byteValue;
                return true;
            case sbyte sbyteValue:
                numericValue = sbyteValue;
                return true;
            case short shortValue:
                numericValue = shortValue;
                return true;
            case ushort ushortValue:
                numericValue = ushortValue;
                return true;
            case int intValue:
                numericValue = intValue;
                return true;
            case uint uintValue:
                numericValue = uintValue;
                return true;
            case long longValue:
                numericValue = longValue;
                return true;
            case ulong ulongValue:
                numericValue = ulongValue;
                return true;
            case float floatValue:
                numericValue = floatValue;
                return true;
            case double doubleValue:
                numericValue = doubleValue;
                return true;
            case decimal decimalValue:
                numericValue = (double)decimalValue;
                return true;
            case string text:
                return double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out numericValue)
                       || double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out numericValue);
            default:
                numericValue = 0d;
                return false;
        }
    }

    private static string FormatValue(object value)
    {
        return value switch
        {
            IFormattable formattable => formattable.ToString(null, CultureInfo.CurrentCulture) ?? string.Empty,
            _ => value.ToString() ?? string.Empty
        };
    }

    private Color GetResourceColor(string key, Color fallback)
    {
        if (TryFindResource(key) is Color color)
        {
            return color;
        }

        if (TryFindResource(key.Replace(".Color", ".Brush")) is SolidColorBrush brush)
        {
            return brush.Color;
        }

        return fallback;
    }

    private static Brush CreateBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
