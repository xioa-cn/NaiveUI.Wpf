using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NEllipsisExpandTrigger
{
    None,
    Click
}

public class NEllipsis : ContentControl
{
    private const double OverflowTolerance = 0.5d;
    private const string MultilineEllipsisSuffix = "...";

    private readonly ToolTip managedToolTip;
    private FormattedText? arrangedText;
    private string cachedText = string.Empty;
    private double cachedLayoutWidth = double.NaN;
    private int? cachedLineClamp;
    private bool cachedExpanded;
    private bool layoutValid;

    private static readonly DependencyPropertyKey HasOverflowPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(HasOverflow),
            typeof(bool),
            typeof(NEllipsis),
            new PropertyMetadata(false));

    private static readonly DependencyPropertyKey IsExpandedPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsExpanded),
            typeof(bool),
            typeof(NEllipsis),
            new PropertyMetadata(false));

    private static readonly DependencyPropertyKey IsTruncatedPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsTruncated),
            typeof(bool),
            typeof(NEllipsis),
            new PropertyMetadata(false));

    static NEllipsis()
    {
        ElementBase.DefaultStyle<NEllipsis>(DefaultStyleKeyProperty);
    }

    public NEllipsis()
    {
        Focusable = false;
        SnapsToDevicePixels = true;
        UseLayoutRounding = true;

        managedToolTip = new ToolTip
        {
            Placement = PlacementMode.Top,
            PlacementTarget = this,
            VerticalOffset = -6
        };
        managedToolTip.SetResourceReference(StyleProperty, "NEllipsis.ToolTipStyle");

        SetValue(ToolTipProperty, managedToolTip);
    }

    public int? LineClamp
    {
        get => (int?)GetValue(LineClampProperty);
        set => SetValue(LineClampProperty, value);
    }

    public static readonly DependencyProperty LineClampProperty =
        ElementBase.Property<NEllipsis, int?>(nameof(LineClampProperty), null, OnEllipsisPropertyChanged);

    public NEllipsisExpandTrigger ExpandTrigger
    {
        get => (NEllipsisExpandTrigger)GetValue(ExpandTriggerProperty);
        set => SetValue(ExpandTriggerProperty, value);
    }

    public static readonly DependencyProperty ExpandTriggerProperty =
        ElementBase.Property<NEllipsis, NEllipsisExpandTrigger>(nameof(ExpandTriggerProperty), NEllipsisExpandTrigger.None, OnEllipsisPropertyChanged);

    public bool Tooltip
    {
        get => (bool)GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
    }

    public static readonly DependencyProperty TooltipProperty =
        ElementBase.Property<NEllipsis, bool>(nameof(TooltipProperty), true, OnEllipsisPropertyChanged);

    public object? TooltipContent
    {
        get => GetValue(TooltipContentProperty);
        set => SetValue(TooltipContentProperty, value);
    }

    public static readonly DependencyProperty TooltipContentProperty =
        ElementBase.Property<NEllipsis, object?>(nameof(TooltipContentProperty), null, OnEllipsisPropertyChanged);

    public bool HasOverflow
    {
        get => (bool)GetValue(HasOverflowProperty);
        private set => SetValue(HasOverflowPropertyKey, value);
    }

    public static readonly DependencyProperty HasOverflowProperty = HasOverflowPropertyKey.DependencyProperty;

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        private set => SetValue(IsExpandedPropertyKey, value);
    }

    public static readonly DependencyProperty IsExpandedProperty = IsExpandedPropertyKey.DependencyProperty;

    public bool IsTruncated
    {
        get => (bool)GetValue(IsTruncatedProperty);
        private set => SetValue(IsTruncatedPropertyKey, value);
    }

    public static readonly DependencyProperty IsTruncatedProperty = IsTruncatedPropertyKey.DependencyProperty;

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        InvalidateEllipsisLayout();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == FontFamilyProperty
            || e.Property == FontSizeProperty
            || e.Property == FontStretchProperty
            || e.Property == FontStyleProperty
            || e.Property == FontWeightProperty
            || e.Property == ForegroundProperty
            || e.Property == FlowDirectionProperty
            || e.Property == WidthProperty
            || e.Property == MinWidthProperty
            || e.Property == MaxWidthProperty)
        {
            InvalidateEllipsisLayout();
        }
    }

    protected override Size MeasureOverride(Size constraint)
    {
        var layoutWidth = ResolveLayoutWidth(constraint.Width);
        EnsureLayout(layoutWidth);

        if (arrangedText is null)
        {
            return new Size(0d, 0d);
        }

        var desiredWidth = Math.Max(arrangedText.Width, arrangedText.WidthIncludingTrailingWhitespace);

        if (!double.IsInfinity(layoutWidth))
        {
            desiredWidth = Math.Min(layoutWidth, desiredWidth);
        }

        if (!double.IsInfinity(constraint.Width))
        {
            desiredWidth = Math.Min(constraint.Width, desiredWidth);
        }

        var desiredHeight = arrangedText.Height;

        if (!double.IsInfinity(constraint.Height))
        {
            desiredHeight = Math.Min(constraint.Height, desiredHeight);
        }

        return new Size(Math.Max(0d, Math.Ceiling(desiredWidth)), Math.Max(0d, Math.Ceiling(desiredHeight)));
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        EnsureLayout(ResolveLayoutWidth(arrangeBounds.Width));
        return arrangeBounds;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (ExpandTrigger != NEllipsisExpandTrigger.Click || (!HasOverflow && !IsExpanded))
        {
            return;
        }

        IsExpanded = !IsExpanded;
        managedToolTip.IsOpen = false;
        InvalidateEllipsisLayout();
        e.Handled = true;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        EnsureLayout(ResolveLayoutWidth(RenderSize.Width));

        if (arrangedText is not null)
        {
            drawingContext.DrawText(arrangedText, new Point(0d, 0d));
        }
    }

    private static void OnEllipsisPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NEllipsis ellipsis)
        {
            return;
        }

        if (e.Property == ExpandTriggerProperty && ellipsis.ExpandTrigger != NEllipsisExpandTrigger.Click && ellipsis.IsExpanded)
        {
            ellipsis.IsExpanded = false;
        }

        ellipsis.InvalidateEllipsisLayout();
    }

    private void InvalidateEllipsisLayout()
    {
        layoutValid = false;
        cachedText = string.Empty;
        cachedLayoutWidth = double.NaN;
        cachedLineClamp = null;
        cachedExpanded = false;

        InvalidateMeasure();
        InvalidateVisual();
    }

    private void EnsureLayout(double layoutWidth)
    {
        var text = ExtractText(Content);
        var normalizedLineClamp = NormalizeLineClamp(LineClamp);

        if (layoutValid
            && string.Equals(cachedText, text, StringComparison.Ordinal)
            && AreClose(cachedLayoutWidth, layoutWidth)
            && cachedLineClamp == normalizedLineClamp
            && cachedExpanded == IsExpanded)
        {
            return;
        }

        var metrics = BuildMetrics(text, layoutWidth, normalizedLineClamp);

        arrangedText = metrics.Display;
        HasOverflow = metrics.HasOverflow;
        IsTruncated = !IsExpanded && metrics.HasOverflow;

        cachedText = text;
        cachedLayoutWidth = layoutWidth;
        cachedLineClamp = normalizedLineClamp;
        cachedExpanded = IsExpanded;
        layoutValid = true;

        UpdateCursorState();
        UpdateTooltipState(text);
    }

    private TextMetrics BuildMetrics(string text, double layoutWidth, int? normalizedLineClamp)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new TextMetrics(CreateBaseFormattedText(string.Empty), false);
        }

        if (normalizedLineClamp.HasValue)
        {
            var fullLayout = CreateMultiLineLayout(text, layoutWidth, normalizedLineClamp.Value, trimmed: false);
            var hasOverflow = fullLayout.Height > GetSingleLineHeight() * normalizedLineClamp.Value + OverflowTolerance;
            var displayLayout = IsExpanded || !hasOverflow
                ? fullLayout
                : CreateCandidateLayout(
                    BuildMultilineTruncatedText(text, layoutWidth, normalizedLineClamp.Value),
                    layoutWidth);

            return new TextMetrics(displayLayout, hasOverflow);
        }

        var singleLineLayout = CreateSingleLineLayout(text, null, trimmed: false);
        var singleLineOverflow = !double.IsInfinity(layoutWidth)
            && singleLineLayout.WidthIncludingTrailingWhitespace > layoutWidth + OverflowTolerance;
        var displaySingleLine = IsExpanded || !singleLineOverflow
            ? singleLineLayout
            : CreateSingleLineLayout(text, layoutWidth, trimmed: true);

        return new TextMetrics(displaySingleLine, singleLineOverflow);
    }

    private FormattedText CreateSingleLineLayout(string text, double? widthConstraint, bool trimmed)
    {
        var formattedText = CreateBaseFormattedText(text);

        if (widthConstraint.HasValue && !double.IsInfinity(widthConstraint.Value))
        {
            formattedText.MaxTextWidth = Math.Max(0d, widthConstraint.Value);
        }

        if (trimmed)
        {
            formattedText.MaxLineCount = 1;
            formattedText.MaxTextHeight = GetSingleLineHeight();
            formattedText.Trimming = TextTrimming.CharacterEllipsis;
        }

        return formattedText;
    }

    private FormattedText CreateMultiLineLayout(string text, double layoutWidth, int lineClamp, bool trimmed)
    {
        var formattedText = CreateBaseFormattedText(text);

        if (!double.IsInfinity(layoutWidth))
        {
            formattedText.MaxTextWidth = Math.Max(0d, layoutWidth);
        }

        if (trimmed)
        {
            formattedText.MaxLineCount = lineClamp;
            formattedText.MaxTextHeight = GetSingleLineHeight() * lineClamp;
            formattedText.Trimming = TextTrimming.CharacterEllipsis;
        }

        return formattedText;
    }

    private string BuildMultilineTruncatedText(string text, double layoutWidth, int lineClamp)
    {
        var maxHeight = GetSingleLineHeight() * lineClamp + OverflowTolerance;
        var low = 0;
        var high = text.Length;
        var best = MultilineEllipsisSuffix;

        while (low <= high)
        {
            var mid = low + ((high - low) / 2);
            var candidate = ComposeMultilineCandidate(text, mid);
            var layout = CreateCandidateLayout(candidate, layoutWidth);

            if (layout.Height <= maxHeight)
            {
                best = candidate;
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return best;
    }

    private FormattedText CreateBaseFormattedText(string text)
    {
        var foreground = Foreground ?? Brushes.Black;
        var fontFamily = FontFamily ?? SystemFonts.MessageFontFamily;
        var fontSize = FontSize > 0d ? FontSize : SystemFonts.MessageFontSize;
        var typeface = new Typeface(fontFamily, FontStyle, FontWeight, FontStretch);
        var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        return new FormattedText(
            text,
            CultureInfo.CurrentUICulture,
            FlowDirection,
            typeface,
            fontSize,
            foreground,
            pixelsPerDip);
    }

    private FormattedText CreateCandidateLayout(string text, double layoutWidth)
    {
        var formattedText = CreateBaseFormattedText(text);

        if (!double.IsInfinity(layoutWidth))
        {
            formattedText.MaxTextWidth = Math.Max(0d, layoutWidth);
        }

        return formattedText;
    }

    private double GetSingleLineHeight()
    {
        return CreateBaseFormattedText("A").Height;
    }

    private void UpdateCursorState()
    {
        SetCurrentValue(CursorProperty, ExpandTrigger == NEllipsisExpandTrigger.Click && HasOverflow && !IsExpanded
            ? Cursors.Hand
            : null);
    }

    private void UpdateTooltipState(string text)
    {
        var enabled = Tooltip && !string.IsNullOrWhiteSpace(text) && !IsExpanded && HasOverflow;
        managedToolTip.Content = TooltipContent ?? CreateDefaultTooltipContent(text);
        ToolTipService.SetIsEnabled(this, enabled);

        if (!enabled)
        {
            managedToolTip.IsOpen = false;
        }
    }

    private double ResolveLayoutWidth(double candidateWidth)
    {
        double resolvedWidth;

        if (!double.IsInfinity(candidateWidth))
        {
            resolvedWidth = Math.Max(0d, candidateWidth);
        }
        else if (!double.IsNaN(Width) && Width > 0d)
        {
            resolvedWidth = Width;
        }
        else if (!double.IsInfinity(MaxWidth) && MaxWidth > 0d)
        {
            resolvedWidth = MaxWidth;
        }
        else if (ActualWidth > 0d)
        {
            resolvedWidth = ActualWidth;
        }
        else
        {
            return double.PositiveInfinity;
        }

        if (!double.IsInfinity(MaxWidth) && MaxWidth > 0d)
        {
            resolvedWidth = Math.Min(resolvedWidth, MaxWidth);
        }

        if (MinWidth > 0d)
        {
            resolvedWidth = Math.Max(resolvedWidth, MinWidth);
        }

        return resolvedWidth;
    }

    private static int? NormalizeLineClamp(int? value)
    {
        return value is > 0 ? value : null;
    }

    private static string ExtractText(object? value)
    {
        return value switch
        {
            null => string.Empty,
            string text => text.Replace("\r\n", "\n"),
            TextBlock textBlock => textBlock.Text.Replace("\r\n", "\n"),
            _ => Convert.ToString(value, CultureInfo.CurrentCulture)?.Replace("\r\n", "\n") ?? string.Empty
        };
    }

    private static bool AreClose(double left, double right)
    {
        if (double.IsInfinity(left) && double.IsInfinity(right))
        {
            return true;
        }

        return Math.Abs(left - right) < 0.5d;
    }

    private static string ComposeMultilineCandidate(string text, int length)
    {
        var candidate = text[..Math.Clamp(length, 0, text.Length)];
        candidate = candidate.TrimEnd(' ', '\t', '\r', '\n');

        return string.IsNullOrEmpty(candidate)
            ? MultilineEllipsisSuffix
            : string.Concat(candidate, MultilineEllipsisSuffix);
    }

    private static TextBlock CreateDefaultTooltipContent(string text)
    {
        return new TextBlock
        {
            MaxWidth = 360d,
            Text = text,
            TextWrapping = TextWrapping.Wrap
        };
    }

    private sealed record TextMetrics(FormattedText Display, bool HasOverflow);
}
