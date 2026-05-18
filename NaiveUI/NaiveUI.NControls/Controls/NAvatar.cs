using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NAvatarShape
{
    Square,
    Round
}

public class NAvatar : ContentControl
{
    private const string ImagePartName = "PART_Image";
    private static readonly CornerRadius SquareCornerRadius = new(4);
    private Image? imagePart;
    private bool isUsingFallbackSource;

    static NAvatar()
    {
        ElementBase.DefaultStyle<NAvatar>(DefaultStyleKeyProperty);
    }

    public NAvatar()
    {
        SizeChanged += (_, _) => UpdateResolvedMetrics();
        Loaded += (_, _) =>
        {
            UpdateResolvedMetrics();
            ResetImageState();
        };
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        ElementBase.Property<NAvatar, double>(nameof(SizeProperty), 34d, OnAvatarLayoutPropertyChanged);

    public NAvatarShape Shape
    {
        get => (NAvatarShape)GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public static readonly DependencyProperty ShapeProperty =
        ElementBase.Property<NAvatar, NAvatarShape>(nameof(ShapeProperty), NAvatarShape.Square, OnAvatarLayoutPropertyChanged);

    public ImageSource? Src
    {
        get => (ImageSource?)GetValue(SrcProperty);
        set => SetValue(SrcProperty, value);
    }

    public static readonly DependencyProperty SrcProperty =
        ElementBase.Property<NAvatar, ImageSource?>(nameof(SrcProperty), null, OnAvatarImageSourceChanged);

    public ImageSource? FallbackSrc
    {
        get => (ImageSource?)GetValue(FallbackSrcProperty);
        set => SetValue(FallbackSrcProperty, value);
    }

    public static readonly DependencyProperty FallbackSrcProperty =
        ElementBase.Property<NAvatar, ImageSource?>(nameof(FallbackSrcProperty), null, OnAvatarImageSourceChanged);

    public object? FallbackContent
    {
        get => GetValue(FallbackContentProperty);
        set => SetValue(FallbackContentProperty, value);
    }

    public static readonly DependencyProperty FallbackContentProperty =
        ElementBase.Property<NAvatar, object?>(nameof(FallbackContentProperty), null, OnAvatarContentPropertyChanged);

    public Stretch ImageStretch
    {
        get => (Stretch)GetValue(ImageStretchProperty);
        set => SetValue(ImageStretchProperty, value);
    }

    public static readonly DependencyProperty ImageStretchProperty =
        ElementBase.Property<NAvatar, Stretch>(nameof(ImageStretchProperty), Stretch.UniformToFill);

    public string Badge
    {
        get => (string)GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    public static readonly DependencyProperty BadgeProperty =
        ElementBase.Property<NAvatar, string>(nameof(BadgeProperty), string.Empty);

    public Brush? PlaceholderForeground
    {
        get => (Brush?)GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }

    public static readonly DependencyProperty PlaceholderForegroundProperty =
        ElementBase.Property<NAvatar, Brush?>(nameof(PlaceholderForegroundProperty), null);

    public Brush? BadgeBackground
    {
        get => (Brush?)GetValue(BadgeBackgroundProperty);
        set => SetValue(BadgeBackgroundProperty, value);
    }

    public static readonly DependencyProperty BadgeBackgroundProperty =
        ElementBase.Property<NAvatar, Brush?>(nameof(BadgeBackgroundProperty), null);

    public Brush? BadgeForeground
    {
        get => (Brush?)GetValue(BadgeForegroundProperty);
        set => SetValue(BadgeForegroundProperty, value);
    }

    public static readonly DependencyProperty BadgeForegroundProperty =
        ElementBase.Property<NAvatar, Brush?>(nameof(BadgeForegroundProperty), null);

    public CornerRadius BadgeCornerRadius
    {
        get => (CornerRadius)GetValue(BadgeCornerRadiusProperty);
        set => SetValue(BadgeCornerRadiusProperty, value);
    }

    public static readonly DependencyProperty BadgeCornerRadiusProperty =
        ElementBase.Property<NAvatar, CornerRadius>(nameof(BadgeCornerRadiusProperty), new CornerRadius(999d));

    public double BadgeFontSize
    {
        get => (double)GetValue(BadgeFontSizeProperty);
        set => SetValue(BadgeFontSizeProperty, value);
    }

    public static readonly DependencyProperty BadgeFontSizeProperty =
        ElementBase.Property<NAvatar, double>(nameof(BadgeFontSizeProperty), double.NaN, OnAvatarLayoutPropertyChanged);

    public Thickness? BadgePadding
    {
        get => (Thickness?)GetValue(BadgePaddingProperty);
        set => SetValue(BadgePaddingProperty, value);
    }

    public static readonly DependencyProperty BadgePaddingProperty =
        ElementBase.Property<NAvatar, Thickness?>(nameof(BadgePaddingProperty), null, OnAvatarLayoutPropertyChanged);

    public Thickness? BadgeMargin
    {
        get => (Thickness?)GetValue(BadgeMarginProperty);
        set => SetValue(BadgeMarginProperty, value);
    }

    public static readonly DependencyProperty BadgeMarginProperty =
        ElementBase.Property<NAvatar, Thickness?>(nameof(BadgeMarginProperty), null, OnAvatarLayoutPropertyChanged);

    public CornerRadius ResolvedCornerRadius
    {
        get => (CornerRadius)GetValue(ResolvedCornerRadiusProperty);
        private set => SetValue(ResolvedCornerRadiusProperty, value);
    }

    public static readonly DependencyProperty ResolvedCornerRadiusProperty =
        ElementBase.Property<NAvatar, CornerRadius>(nameof(ResolvedCornerRadiusProperty), SquareCornerRadius);

    public double ResolvedFontSize
    {
        get => (double)GetValue(ResolvedFontSizeProperty);
        private set => SetValue(ResolvedFontSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedFontSizeProperty =
        ElementBase.Property<NAvatar, double>(nameof(ResolvedFontSizeProperty), 14d);

    public Thickness ResolvedContentMargin
    {
        get => (Thickness)GetValue(ResolvedContentMarginProperty);
        private set => SetValue(ResolvedContentMarginProperty, value);
    }

    public static readonly DependencyProperty ResolvedContentMarginProperty =
        ElementBase.Property<NAvatar, Thickness>(nameof(ResolvedContentMarginProperty), new Thickness(5));

    public Thickness ResolvedPlaceholderMargin
    {
        get => (Thickness)GetValue(ResolvedPlaceholderMarginProperty);
        private set => SetValue(ResolvedPlaceholderMarginProperty, value);
    }

    public static readonly DependencyProperty ResolvedPlaceholderMarginProperty =
        ElementBase.Property<NAvatar, Thickness>(nameof(ResolvedPlaceholderMarginProperty), new Thickness(7));

    public Thickness ResolvedBadgeMargin
    {
        get => (Thickness)GetValue(ResolvedBadgeMarginProperty);
        private set => SetValue(ResolvedBadgeMarginProperty, value);
    }

    public static readonly DependencyProperty ResolvedBadgeMarginProperty =
        ElementBase.Property<NAvatar, Thickness>(nameof(ResolvedBadgeMarginProperty), new Thickness(0, -5, -8, 0));

    public Thickness ResolvedBadgePadding
    {
        get => (Thickness)GetValue(ResolvedBadgePaddingProperty);
        private set => SetValue(ResolvedBadgePaddingProperty, value);
    }

    public static readonly DependencyProperty ResolvedBadgePaddingProperty =
        ElementBase.Property<NAvatar, Thickness>(nameof(ResolvedBadgePaddingProperty), new Thickness(5, 1, 5, 1));

    public double ResolvedBadgeMinSize
    {
        get => (double)GetValue(ResolvedBadgeMinSizeProperty);
        private set => SetValue(ResolvedBadgeMinSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedBadgeMinSizeProperty =
        ElementBase.Property<NAvatar, double>(nameof(ResolvedBadgeMinSizeProperty), 18d);

    public double ResolvedBadgeFontSize
    {
        get => (double)GetValue(ResolvedBadgeFontSizeProperty);
        private set => SetValue(ResolvedBadgeFontSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedBadgeFontSizeProperty =
        ElementBase.Property<NAvatar, double>(nameof(ResolvedBadgeFontSizeProperty), 10d);

    public ImageSource? ResolvedImageSource
    {
        get => (ImageSource?)GetValue(ResolvedImageSourceProperty);
        private set => SetValue(ResolvedImageSourceProperty, value);
    }

    public static readonly DependencyProperty ResolvedImageSourceProperty =
        ElementBase.Property<NAvatar, ImageSource?>(nameof(ResolvedImageSourceProperty), null);

    public bool IsImageVisible
    {
        get => (bool)GetValue(IsImageVisibleProperty);
        private set => SetValue(IsImageVisibleProperty, value);
    }

    public static readonly DependencyProperty IsImageVisibleProperty =
        ElementBase.Property<NAvatar, bool>(nameof(IsImageVisibleProperty), false);

    public bool HasCustomContent
    {
        get => (bool)GetValue(HasCustomContentProperty);
        private set => SetValue(HasCustomContentProperty, value);
    }

    public static readonly DependencyProperty HasCustomContentProperty =
        ElementBase.Property<NAvatar, bool>(nameof(HasCustomContentProperty), false);

    public string DisplayTextContent
    {
        get => (string)GetValue(DisplayTextContentProperty);
        private set => SetValue(DisplayTextContentProperty, value);
    }

    public static readonly DependencyProperty DisplayTextContentProperty =
        ElementBase.Property<NAvatar, string>(nameof(DisplayTextContentProperty), string.Empty);

    public object? DisplayVisualContent
    {
        get => GetValue(DisplayVisualContentProperty);
        private set => SetValue(DisplayVisualContentProperty, value);
    }

    public static readonly DependencyProperty DisplayVisualContentProperty =
        ElementBase.Property<NAvatar, object?>(nameof(DisplayVisualContentProperty), null);

    public bool HasTextContent
    {
        get => (bool)GetValue(HasTextContentProperty);
        private set => SetValue(HasTextContentProperty, value);
    }

    public static readonly DependencyProperty HasTextContentProperty =
        ElementBase.Property<NAvatar, bool>(nameof(HasTextContentProperty), false);

    public bool ShowPlaceholder
    {
        get => (bool)GetValue(ShowPlaceholderProperty);
        private set => SetValue(ShowPlaceholderProperty, value);
    }

    public static readonly DependencyProperty ShowPlaceholderProperty =
        ElementBase.Property<NAvatar, bool>(nameof(ShowPlaceholderProperty), true);

    public bool IsCompactBadge
    {
        get => (bool)GetValue(IsCompactBadgeProperty);
        private set => SetValue(IsCompactBadgeProperty, value);
    }

    public static readonly DependencyProperty IsCompactBadgeProperty =
        ElementBase.Property<NAvatar, bool>(nameof(IsCompactBadgeProperty), true);

    public override void OnApplyTemplate()
    {
        if (imagePart is not null)
        {
            imagePart.ImageFailed -= HandleImageFailed;
        }

        base.OnApplyTemplate();

        imagePart = GetTemplateChild(ImagePartName) as Image;
        if (imagePart is not null)
        {
            imagePart.ImageFailed += HandleImageFailed;
        }

        ResetImageState();
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        UpdateDisplayState();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == FontSizeProperty)
        {
            UpdateResolvedMetrics();
        }
        else if (e.Property == BadgeProperty)
        {
            UpdateBadgeState();
        }
    }

    private static void OnAvatarLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NAvatar avatar)
        {
            avatar.UpdateResolvedMetrics();
        }
    }

    private static void OnAvatarImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NAvatar avatar)
        {
            avatar.ResetImageState();
        }
    }

    private static void OnAvatarContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NAvatar avatar)
        {
            avatar.UpdateDisplayState();
        }
    }

    private void ResetImageState()
    {
        isUsingFallbackSource = false;
        ResolvedImageSource = Src;
        UpdateBadgeState();
        UpdateDisplayState();
    }

    private void UpdateResolvedMetrics()
    {
        ResolvedCornerRadius = Shape == NAvatarShape.Round
            ? new CornerRadius(Size / 2d)
            : SquareCornerRadius;

        var localFontSize = ReadLocalValue(FontSizeProperty);
        ResolvedFontSize = localFontSize == DependencyProperty.UnsetValue
            ? Math.Max(12d, Math.Round(Size * 0.4d, 0))
            : FontSize;

        var contentInset = Math.Max(4d, Math.Round(Size * 0.15d, 0));
        var placeholderInset = Math.Max(6d, Math.Round(Size * 0.2d, 0));
        ResolvedContentMargin = new Thickness(contentInset);
        ResolvedPlaceholderMargin = new Thickness(placeholderInset);
        ResolvedBadgeMinSize = Math.Max(18d, Math.Round(Size * 0.52d, 0));
        ResolvedBadgeFontSize = double.IsNaN(BadgeFontSize)
            ? Math.Max(9d, Math.Round(Size * 0.24d, 0))
            : BadgeFontSize;
        var autoBadgePadding = new Thickness(
            Math.Max(5d, Math.Round(Size * 0.15d, 0)),
            1,
            Math.Max(5d, Math.Round(Size * 0.15d, 0)),
            1);
        var autoBadgeMargin = new Thickness(
            0,
            -Math.Round(Size * 0.15d, 0),
            -Math.Round(Size * 0.24d, 0),
            0);

        ResolvedBadgePadding = BadgePadding ?? autoBadgePadding;
        ResolvedBadgeMargin = BadgeMargin ?? autoBadgeMargin;
    }

    private void UpdateBadgeState()
    {
        var trimmedBadge = Badge?.Trim() ?? string.Empty;
        IsCompactBadge = trimmedBadge.Length <= 1;
    }

    private void UpdateDisplayState()
    {
        var primaryContent = DescribeContent(Content);
        var fallbackContent = DescribeContent(FallbackContent);

        var shouldShowImage = ResolvedImageSource is not null;
        IsImageVisible = shouldShowImage;

        DisplayTextContent = string.Empty;
        DisplayVisualContent = null;
        HasTextContent = false;
        HasCustomContent = false;

        if (shouldShowImage)
        {
            ShowPlaceholder = false;
            return;
        }

        if (primaryContent.HasText)
        {
            DisplayTextContent = primaryContent.Text!;
            HasTextContent = true;
        }
        else if (primaryContent.HasVisual)
        {
            DisplayVisualContent = primaryContent.Visual;
            HasCustomContent = true;
        }
        else if (!shouldShowImage && fallbackContent.HasText)
        {
            DisplayTextContent = fallbackContent.Text!;
            HasTextContent = true;
        }
        else if (!shouldShowImage && fallbackContent.HasVisual)
        {
            DisplayVisualContent = fallbackContent.Visual;
            HasCustomContent = true;
        }

        ShowPlaceholder = !IsImageVisible && !HasTextContent && !HasCustomContent;
    }

    private void HandleImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
        if (!isUsingFallbackSource && FallbackSrc is not null)
        {
            isUsingFallbackSource = true;
            ResolvedImageSource = FallbackSrc;
            UpdateDisplayState();
            return;
        }

        ResolvedImageSource = null;
        UpdateDisplayState();
    }

    private static AvatarContentState DescribeContent(object? content)
    {
        return content switch
        {
            null => AvatarContentState.Empty,
            string text when string.IsNullOrWhiteSpace(text) => AvatarContentState.Empty,
            string text => new AvatarContentState(true, text, null),
            _ => new AvatarContentState(true, null, content)
        };
    }

    private readonly record struct AvatarContentState(bool HasContent, string? Text, object? Visual)
    {
        public static AvatarContentState Empty => new(false, null, null);

        public bool HasText => Text is not null;

        public bool HasVisual => Visual is not null;
    }
}
