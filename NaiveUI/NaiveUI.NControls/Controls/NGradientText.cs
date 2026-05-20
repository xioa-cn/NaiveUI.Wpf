using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NaiveUI.NControls.Themes;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NGradientTextType
{
    Primary,
    Info,
    Success,
    Warning,
    Error,
    Danger
}

public sealed class NGradientTextGradient
{
    public string From { get; set; } = string.Empty;

    public string To { get; set; } = string.Empty;

    public double Deg { get; set; }
}

public class NGradientText : TextBlock
{
    private const double DefaultRotate = 252d;
    private const double DefaultStartAlpha = 0.6d;

    static NGradientText()
    {
        ElementBase.DefaultStyle<NGradientText>(DefaultStyleKeyProperty);
    }

    public NGradientText()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        ApplySizeOverride();
        UpdateResolvedForeground();
    }

    public object? Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        ElementBase.Property<NGradientText, object?>(nameof(SizeProperty), null, OnAppearancePropertyChanged);

    public NGradientTextType Type
    {
        get => (NGradientTextType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public static readonly DependencyProperty TypeProperty =
        ElementBase.Property<NGradientText, NGradientTextType>(nameof(TypeProperty), NGradientTextType.Primary, OnAppearancePropertyChanged);

    public object? Gradient
    {
        get => GetValue(GradientProperty);
        set => SetValue(GradientProperty, value);
    }

    public static readonly DependencyProperty GradientProperty =
        ElementBase.Property<NGradientText, object?>(nameof(GradientProperty), null, OnAppearancePropertyChanged);

    public object? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly DependencyProperty ColorProperty =
        ElementBase.Property<NGradientText, object?>(nameof(ColorProperty), null, OnAppearancePropertyChanged);

    private static void OnAppearancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NGradientText gradientText)
        {
            return;
        }

        if (e.Property == SizeProperty)
        {
            gradientText.ApplySizeOverride();
            return;
        }

        gradientText.UpdateResolvedForeground();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged += HandleThemeChanged;
        ApplySizeOverride();
        UpdateResolvedForeground();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged -= HandleThemeChanged;
    }

    private void HandleThemeChanged(ThemeMode mode)
    {
        UpdateResolvedForeground();
    }

    private void ApplySizeOverride()
    {
        if (!TryResolveSize(Size, out var size))
        {
            return;
        }

        SetCurrentValue(FontSizeProperty, size);
    }

    private void UpdateResolvedForeground()
    {
        var brush = TryCreateCustomBrush(Color)
                    ?? TryCreateCustomBrush(Gradient)
                    ?? CreateThemeBrush();

        SetCurrentValue(ForegroundProperty, brush);
    }

    private Brush CreateThemeBrush()
    {
        var endColor = GetThemeEndColor();
        var startColor = ApplyAlpha(endColor, DefaultStartAlpha);
        return CreateLinearGradientBrush(startColor, endColor, DefaultRotate);
    }

    private Color GetThemeEndColor()
    {
        var resourceKey = GetResourceColorKey(Type);
        return TryGetResourceColor(resourceKey, out var color)
            ? color
            : Colors.Black;
    }

    private static string GetResourceColorKey(NGradientTextType type)
    {
        return type switch
        {
            NGradientTextType.Info => "Info.First.Color",
            NGradientTextType.Success => "Success.First.Color",
            NGradientTextType.Warning => "Warning.First.Color",
            NGradientTextType.Error => "Error.First.Color",
            NGradientTextType.Danger => "Error.First.Color",
            _ => "Primary.First.Color"
        };
    }

    private bool TryGetResourceColor(string resourceKey, out Color color)
    {
        if (TryFindResource(resourceKey) is Color directColor)
        {
            color = directColor;
            return true;
        }

        if (TryFindResource(resourceKey) is SolidColorBrush brush)
        {
            color = brush.Color;
            return true;
        }

        if (Application.Current?.TryFindResource(resourceKey) is Color appColor)
        {
            color = appColor;
            return true;
        }

        if (Application.Current?.TryFindResource(resourceKey) is SolidColorBrush appBrush)
        {
            color = appBrush.Color;
            return true;
        }

        color = Colors.Black;
        return false;
    }

    private static Brush? TryCreateCustomBrush(object? value)
    {
        return value switch
        {
            null => null,
            Brush brush => brush,
            NGradientTextGradient gradient => TryCreateBrushFromDefinition(gradient),
            string text => TryCreateBrushFromString(text),
            _ => null
        };
    }

    private static Brush? TryCreateBrushFromDefinition(NGradientTextGradient definition)
    {
        if (!TryParseColor(definition.From, out var fromColor) || !TryParseColor(definition.To, out var toColor))
        {
            return null;
        }

        return CreateLinearGradientBrush(fromColor, toColor, definition.Deg);
    }

    private static Brush? TryCreateBrushFromString(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var trimmed = text.Trim();

        if (trimmed.StartsWith("linear-gradient(", StringComparison.OrdinalIgnoreCase))
        {
            return TryCreateBrushFromLinearGradient(trimmed);
        }

        if (TryParseColor(trimmed, out var singleColor))
        {
            return new SolidColorBrush(singleColor);
        }

        return null;
    }

    private static Brush? TryCreateBrushFromLinearGradient(string text)
    {
        if (!text.EndsWith(")", StringComparison.Ordinal) || text.Length <= "linear-gradient()".Length)
        {
            return null;
        }

        var innerText = text["linear-gradient(".Length..^1];
        var segments = SplitTopLevelSegments(innerText);
        if (segments.Count < 2)
        {
            return null;
        }

        var segmentIndex = 0;
        var angle = 180d;

        if (TryParseAngle(segments[0], out var parsedAngle))
        {
            angle = parsedAngle;
            segmentIndex = 1;
        }

        var stops = new List<GradientStop>();
        for (var i = segmentIndex; i < segments.Count; i++)
        {
            if (!TryParseGradientStop(segments[i], out var gradientStop))
            {
                return null;
            }

            stops.Add(gradientStop);
        }

        if (stops.Count == 0)
        {
            return null;
        }

        NormalizeGradientStopOffsets(stops);

        var brush = CreateLinearGradientBrush(angle);
        foreach (var stop in stops)
        {
            brush.GradientStops.Add(stop);
        }

        return brush;
    }

    private static bool TryParseGradientStop(string text, out GradientStop gradientStop)
    {
        gradientStop = new GradientStop();
        var trimmed = text.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return false;
        }

        var splitIndex = FindOffsetSeparatorIndex(trimmed);
        var colorPart = splitIndex >= 0 ? trimmed[..splitIndex].Trim() : trimmed;
        var offsetPart = splitIndex >= 0 ? trimmed[(splitIndex + 1)..].Trim() : string.Empty;

        if (!TryParseColor(colorPart, out var color))
        {
            return false;
        }

        var offset = double.NaN;
        if (!string.IsNullOrWhiteSpace(offsetPart))
        {
            if (!TryParsePercentage(offsetPart, out offset))
            {
                return false;
            }
        }

        gradientStop = new GradientStop(color, offset);
        return true;
    }

    private static int FindOffsetSeparatorIndex(string text)
    {
        var depth = 0;
        for (var i = text.Length - 1; i >= 0; i--)
        {
            var ch = text[i];
            switch (ch)
            {
                case ')':
                    depth++;
                    break;
                case '(':
                    depth--;
                    break;
                default:
                    if (depth == 0 && char.IsWhiteSpace(ch))
                    {
                        return i;
                    }
                    break;
            }
        }

        return -1;
    }

    private static void NormalizeGradientStopOffsets(IList<GradientStop> stops)
    {
        if (stops.Count == 1)
        {
            stops[0].Offset = 0d;
            return;
        }

        var missingIndices = new List<int>();
        for (var i = 0; i < stops.Count; i++)
        {
            if (double.IsNaN(stops[i].Offset))
            {
                missingIndices.Add(i);
            }
        }

        if (missingIndices.Count == 0)
        {
            return;
        }

        for (var i = 0; i < stops.Count; i++)
        {
            if (!double.IsNaN(stops[i].Offset))
            {
                continue;
            }

            if (i == 0)
            {
                stops[i].Offset = 0d;
                continue;
            }

            if (i == stops.Count - 1)
            {
                stops[i].Offset = 1d;
                continue;
            }

            var previousIndex = i - 1;
            while (previousIndex >= 0 && double.IsNaN(stops[previousIndex].Offset))
            {
                previousIndex--;
            }

            var nextIndex = i + 1;
            while (nextIndex < stops.Count && double.IsNaN(stops[nextIndex].Offset))
            {
                nextIndex++;
            }

            var previousOffset = previousIndex >= 0 ? stops[previousIndex].Offset : 0d;
            var nextOffset = nextIndex < stops.Count ? stops[nextIndex].Offset : 1d;
            var span = nextIndex - previousIndex;
            var step = span <= 0 ? 0d : (nextOffset - previousOffset) / span;
            stops[i].Offset = previousOffset + (i - previousIndex) * step;
        }
    }

    private static List<string> SplitTopLevelSegments(string text)
    {
        var segments = new List<string>();
        var builder = new StringBuilder();
        var depth = 0;

        foreach (var ch in text)
        {
            switch (ch)
            {
                case '(':
                    depth++;
                    builder.Append(ch);
                    break;
                case ')':
                    depth = Math.Max(0, depth - 1);
                    builder.Append(ch);
                    break;
                case ',' when depth == 0:
                    segments.Add(builder.ToString().Trim());
                    builder.Clear();
                    break;
                default:
                    builder.Append(ch);
                    break;
            }
        }

        if (builder.Length > 0)
        {
            segments.Add(builder.ToString().Trim());
        }

        return segments;
    }

    private static bool TryParseAngle(string text, out double angle)
    {
        var normalized = text.Trim();

        if (normalized.EndsWith("deg", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[..^3].Trim();
        }

        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out angle);
    }

    private static bool TryParsePercentage(string text, out double offset)
    {
        var normalized = text.Trim();
        if (!normalized.EndsWith("%", StringComparison.Ordinal))
        {
            offset = 0d;
            return false;
        }

        normalized = normalized[..^1].Trim();
        if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var percentage))
        {
            offset = 0d;
            return false;
        }

        offset = percentage / 100d;
        return true;
    }

    private static bool TryResolveSize(object? value, out double size)
    {
        switch (value)
        {
            case null:
                size = 0d;
                return false;
            case double doubleValue when !double.IsNaN(doubleValue) && doubleValue > 0d:
                size = doubleValue;
                return true;
            case float floatValue when !float.IsNaN(floatValue) && floatValue > 0f:
                size = floatValue;
                return true;
            case int intValue when intValue > 0:
                size = intValue;
                return true;
            case long longValue when longValue > 0:
                size = longValue;
                return true;
            case string text:
            {
                var normalized = text.Trim();
                if (normalized.EndsWith("px", StringComparison.OrdinalIgnoreCase))
                {
                    normalized = normalized[..^2].Trim();
                }

                if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedSize)
                    && parsedSize > 0d)
                {
                    size = parsedSize;
                    return true;
                }

                break;
            }
        }

        size = 0d;
        return false;
    }

    private static bool TryParseColor(string text, out Color color)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase) && trimmed.EndsWith(")", StringComparison.Ordinal))
        {
            return TryParseRgbColor(trimmed[4..^1], false, out color);
        }

        if (trimmed.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase) && trimmed.EndsWith(")", StringComparison.Ordinal))
        {
            return TryParseRgbColor(trimmed[5..^1], true, out color);
        }

        try
        {
            var converted = ColorConverter.ConvertFromString(trimmed);
            if (converted is Color parsedColor)
            {
                color = parsedColor;
                return true;
            }
        }
        catch
        {
        }

        color = Colors.Transparent;
        return false;
    }

    private static bool TryParseRgbColor(string text, bool hasAlpha, out Color color)
    {
        var parts = SplitTopLevelSegments(text);
        if (parts.Count != (hasAlpha ? 4 : 3))
        {
            color = Colors.Transparent;
            return false;
        }

        if (!TryParseByte(parts[0], out var red)
            || !TryParseByte(parts[1], out var green)
            || !TryParseByte(parts[2], out var blue))
        {
            color = Colors.Transparent;
            return false;
        }

        var alpha = (byte)255;
        if (hasAlpha && !TryParseAlpha(parts[3], out alpha))
        {
            color = Colors.Transparent;
            return false;
        }

        color = System.Windows.Media.Color.FromArgb(alpha, red, green, blue);
        return true;
    }

    private static bool TryParseByte(string text, out byte value)
    {
        if (byte.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = 0;
        return false;
    }

    private static bool TryParseAlpha(string text, out byte alpha)
    {
        var normalized = text.Trim();
        if (normalized.EndsWith("%", StringComparison.Ordinal))
        {
            if (double.TryParse(normalized[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var percentage))
            {
                alpha = (byte)Math.Clamp((int)Math.Round(percentage / 100d * 255d), 0, 255);
                return true;
            }

            alpha = 255;
            return false;
        }

        if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var alphaValue))
        {
            if (alphaValue <= 1d)
            {
                alpha = (byte)Math.Clamp((int)Math.Round(alphaValue * 255d), 0, 255);
                return true;
            }

            alpha = (byte)Math.Clamp((int)Math.Round(alphaValue), 0, 255);
            return true;
        }

        alpha = 255;
        return false;
    }

    private static LinearGradientBrush CreateLinearGradientBrush(Color fromColor, Color toColor, double angle)
    {
        var brush = CreateLinearGradientBrush(angle);
        brush.GradientStops.Add(new GradientStop(fromColor, 0d));
        brush.GradientStops.Add(new GradientStop(toColor, 1d));
        return brush;
    }

    private static LinearGradientBrush CreateLinearGradientBrush(double angle)
    {
        var (startPoint, endPoint) = ResolveGradientPoints(angle);
        return new LinearGradientBrush
        {
            StartPoint = startPoint,
            EndPoint = endPoint,
            MappingMode = BrushMappingMode.RelativeToBoundingBox
        };
    }

    private static (Point StartPoint, Point EndPoint) ResolveGradientPoints(double angle)
    {
        var radians = (angle - 90d) * Math.PI / 180d;
        var x = Math.Cos(radians);
        var y = Math.Sin(radians);

        var scale = 0.5d / Math.Max(Math.Abs(x), Math.Abs(y));
        var startPoint = new Point(0.5d - x * scale, 0.5d - y * scale);
        var endPoint = new Point(0.5d + x * scale, 0.5d + y * scale);
        return (startPoint, endPoint);
    }

    private static Color ApplyAlpha(Color color, double alpha)
    {
        return System.Windows.Media.Color.FromArgb(
            (byte)Math.Clamp((int)Math.Round(alpha * 255d), 0, 255),
            color.R,
            color.G,
            color.B);
    }
}
