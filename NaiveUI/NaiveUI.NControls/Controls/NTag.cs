using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using NaiveUI.NControls.Themes;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NTagType
{
    Default,
    Primary,
    Success,
    Info,
    Warning,
    Error
}

public enum NTagSize
{
    Tiny,
    Small,
    Medium,
    Large
}

public sealed class NTagColor
{
    public string? Color { get; set; }

    public string? BorderColor { get; set; }

    public string? TextColor { get; set; }
}

public class NTag : ContentControl
{
    private const string CloseButtonPartName = "PART_CloseButton";
    private static readonly CornerRadius DefaultCornerRadius = new(4d);
    private ButtonBase? closeButtonPart;
    private bool isPointerPressed;
    private bool isSyncingDisabled;

    static NTag()
    {
        ElementBase.DefaultStyle<NTag>(DefaultStyleKeyProperty);
    }

    public NTag()
    {
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        UpdateResolvedState();
    }

    public NTagType Type
    {
        get => (NTagType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public static readonly DependencyProperty TypeProperty =
        ElementBase.Property<NTag, NTagType>(nameof(TypeProperty), NTagType.Default, OnTagAppearancePropertyChanged);

    public NTagSize Size
    {
        get => (NTagSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        ElementBase.Property<NTag, NTagSize>(nameof(SizeProperty), NTagSize.Medium, OnTagAppearancePropertyChanged);

    public double TextFontSize
    {
        get => (double)GetValue(TextFontSizeProperty);
        set => SetValue(TextFontSizeProperty, value);
    }

    public static readonly DependencyProperty TextFontSizeProperty =
        ElementBase.Property<NTag, double>(nameof(TextFontSizeProperty), double.NaN, OnTagAppearancePropertyChanged);

    public bool AutoScaleIconAndAvatar
    {
        get => (bool)GetValue(AutoScaleIconAndAvatarProperty);
        set => SetValue(AutoScaleIconAndAvatarProperty, value);
    }

    public static readonly DependencyProperty AutoScaleIconAndAvatarProperty =
        ElementBase.Property<NTag, bool>(nameof(AutoScaleIconAndAvatarProperty), false, OnTagAppearancePropertyChanged);

    public bool Bordered
    {
        get => (bool)GetValue(BorderedProperty);
        set => SetValue(BorderedProperty, value);
    }

    public static readonly DependencyProperty BorderedProperty =
        ElementBase.Property<NTag, bool>(nameof(BorderedProperty), true, OnTagAppearancePropertyChanged);

    public bool Checked
    {
        get => (bool)GetValue(CheckedProperty);
        set => SetValue(CheckedProperty, value);
    }

    public static readonly DependencyProperty CheckedProperty =
        DependencyProperty.Register(
            nameof(Checked),
            typeof(bool),
            typeof(NTag),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnCheckedPropertyChanged));

    public bool Checkable
    {
        get => (bool)GetValue(CheckableProperty);
        set => SetValue(CheckableProperty, value);
    }

    public static readonly DependencyProperty CheckableProperty =
        ElementBase.Property<NTag, bool>(nameof(CheckableProperty), false, OnTagAppearancePropertyChanged);

    public bool Strong
    {
        get => (bool)GetValue(StrongProperty);
        set => SetValue(StrongProperty, value);
    }

    public static readonly DependencyProperty StrongProperty =
        ElementBase.Property<NTag, bool>(nameof(StrongProperty), false, OnTagAppearancePropertyChanged);

    public bool TriggerClickOnClose
    {
        get => (bool)GetValue(TriggerClickOnCloseProperty);
        set => SetValue(TriggerClickOnCloseProperty, value);
    }

    public static readonly DependencyProperty TriggerClickOnCloseProperty =
        ElementBase.Property<NTag, bool>(nameof(TriggerClickOnCloseProperty), false);

    public bool Closable
    {
        get => (bool)GetValue(ClosableProperty);
        set => SetValue(ClosableProperty, value);
    }

    public static readonly DependencyProperty ClosableProperty =
        ElementBase.Property<NTag, bool>(nameof(ClosableProperty), false, OnTagAppearancePropertyChanged);

    public bool Disabled
    {
        get => (bool)GetValue(DisabledProperty);
        set => SetValue(DisabledProperty, value);
    }

    public static readonly DependencyProperty DisabledProperty =
        ElementBase.Property<NTag, bool>(nameof(DisabledProperty), false, OnDisabledPropertyChanged);

    public bool Round
    {
        get => (bool)GetValue(RoundProperty);
        set => SetValue(RoundProperty, value);
    }

    public static readonly DependencyProperty RoundProperty =
        ElementBase.Property<NTag, bool>(nameof(RoundProperty), false, OnTagAppearancePropertyChanged);

    public NTagColor? Color
    {
        get => (NTagColor?)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly DependencyProperty ColorProperty =
        ElementBase.Property<NTag, NTagColor?>(nameof(ColorProperty), null, OnTagAppearancePropertyChanged);

    public object? AvatarContent
    {
        get => GetValue(AvatarContentProperty);
        set => SetValue(AvatarContentProperty, value);
    }

    public static readonly DependencyProperty AvatarContentProperty =
        ElementBase.Property<NTag, object?>(nameof(AvatarContentProperty), null, OnSupplementalContentPropertyChanged);

    public object? IconContent
    {
        get => GetValue(IconContentProperty);
        set => SetValue(IconContentProperty, value);
    }

    public static readonly DependencyProperty IconContentProperty =
        ElementBase.Property<NTag, object?>(nameof(IconContentProperty), null, OnSupplementalContentPropertyChanged);

    public object? DisplayAvatarContent
    {
        get => GetValue(DisplayAvatarContentProperty);
        private set => SetValue(DisplayAvatarContentProperty, value);
    }

    public static readonly DependencyProperty DisplayAvatarContentProperty =
        ElementBase.Property<NTag, object?>(nameof(DisplayAvatarContentProperty), null);

    public object? DisplayIconContent
    {
        get => GetValue(DisplayIconContentProperty);
        private set => SetValue(DisplayIconContentProperty, value);
    }

    public static readonly DependencyProperty DisplayIconContentProperty =
        ElementBase.Property<NTag, object?>(nameof(DisplayIconContentProperty), null);

    public bool ShowAvatar
    {
        get => (bool)GetValue(ShowAvatarProperty);
        private set => SetValue(ShowAvatarProperty, value);
    }

    public static readonly DependencyProperty ShowAvatarProperty =
        ElementBase.Property<NTag, bool>(nameof(ShowAvatarProperty), false);

    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        private set => SetValue(ShowIconProperty, value);
    }

    public static readonly DependencyProperty ShowIconProperty =
        ElementBase.Property<NTag, bool>(nameof(ShowIconProperty), false);

    public bool ShowCloseButton
    {
        get => (bool)GetValue(ShowCloseButtonProperty);
        private set => SetValue(ShowCloseButtonProperty, value);
    }

    public static readonly DependencyProperty ShowCloseButtonProperty =
        ElementBase.Property<NTag, bool>(nameof(ShowCloseButtonProperty), false);

    public bool ShowBorder
    {
        get => (bool)GetValue(ShowBorderProperty);
        private set => SetValue(ShowBorderProperty, value);
    }

    public static readonly DependencyProperty ShowBorderProperty =
        ElementBase.Property<NTag, bool>(nameof(ShowBorderProperty), true);

    public double ResolvedHeight
    {
        get => (double)GetValue(ResolvedHeightProperty);
        private set => SetValue(ResolvedHeightProperty, value);
    }

    public static readonly DependencyProperty ResolvedHeightProperty =
        ElementBase.Property<NTag, double>(nameof(ResolvedHeightProperty), 34d);

    public double ResolvedFontSize
    {
        get => (double)GetValue(ResolvedFontSizeProperty);
        private set => SetValue(ResolvedFontSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedFontSizeProperty =
        ElementBase.Property<NTag, double>(nameof(ResolvedFontSizeProperty), 14d);

    public FontWeight ResolvedFontWeight
    {
        get => (FontWeight)GetValue(ResolvedFontWeightProperty);
        private set => SetValue(ResolvedFontWeightProperty, value);
    }

    public static readonly DependencyProperty ResolvedFontWeightProperty =
        ElementBase.Property<NTag, FontWeight>(nameof(ResolvedFontWeightProperty), FontWeights.Normal);

    public Thickness ResolvedPadding
    {
        get => (Thickness)GetValue(ResolvedPaddingProperty);
        private set => SetValue(ResolvedPaddingProperty, value);
    }

    public static readonly DependencyProperty ResolvedPaddingProperty =
        ElementBase.Property<NTag, Thickness>(nameof(ResolvedPaddingProperty), new Thickness(7, 0, 7, 0));

    public CornerRadius ResolvedCornerRadius
    {
        get => (CornerRadius)GetValue(ResolvedCornerRadiusProperty);
        private set => SetValue(ResolvedCornerRadiusProperty, value);
    }

    public static readonly DependencyProperty ResolvedCornerRadiusProperty =
        ElementBase.Property<NTag, CornerRadius>(nameof(ResolvedCornerRadiusProperty), DefaultCornerRadius);

    public Brush ResolvedBackground
    {
        get => (Brush)GetValue(ResolvedBackgroundProperty);
        private set => SetValue(ResolvedBackgroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedBackgroundProperty =
        ElementBase.Property<NTag, Brush>(nameof(ResolvedBackgroundProperty), Brushes.Transparent);

    public Brush ResolvedBorderBrush
    {
        get => (Brush)GetValue(ResolvedBorderBrushProperty);
        private set => SetValue(ResolvedBorderBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedBorderBrushProperty =
        ElementBase.Property<NTag, Brush>(nameof(ResolvedBorderBrushProperty), Brushes.Transparent);

    public double ResolvedCloseSize
    {
        get => (double)GetValue(ResolvedCloseSizeProperty);
        private set => SetValue(ResolvedCloseSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedCloseSizeProperty =
        ElementBase.Property<NTag, double>(nameof(ResolvedCloseSizeProperty), 18d);

    public double ResolvedCloseIconSize
    {
        get => (double)GetValue(ResolvedCloseIconSizeProperty);
        private set => SetValue(ResolvedCloseIconSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedCloseIconSizeProperty =
        ElementBase.Property<NTag, double>(nameof(ResolvedCloseIconSizeProperty), 14d);

    public CornerRadius ResolvedCloseCornerRadius
    {
        get => (CornerRadius)GetValue(ResolvedCloseCornerRadiusProperty);
        private set => SetValue(ResolvedCloseCornerRadiusProperty, value);
    }

    public static readonly DependencyProperty ResolvedCloseCornerRadiusProperty =
        ElementBase.Property<NTag, CornerRadius>(nameof(ResolvedCloseCornerRadiusProperty), new CornerRadius(2d));

    public Brush ResolvedCloseIconBrush
    {
        get => (Brush)GetValue(ResolvedCloseIconBrushProperty);
        private set => SetValue(ResolvedCloseIconBrushProperty, value);
    }

    public static readonly DependencyProperty ResolvedCloseIconBrushProperty =
        ElementBase.Property<NTag, Brush>(nameof(ResolvedCloseIconBrushProperty), Brushes.Transparent);

    public Brush ResolvedCloseHoverBackground
    {
        get => (Brush)GetValue(ResolvedCloseHoverBackgroundProperty);
        private set => SetValue(ResolvedCloseHoverBackgroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedCloseHoverBackgroundProperty =
        ElementBase.Property<NTag, Brush>(nameof(ResolvedCloseHoverBackgroundProperty), Brushes.Transparent);

    public Brush ResolvedClosePressedBackground
    {
        get => (Brush)GetValue(ResolvedClosePressedBackgroundProperty);
        private set => SetValue(ResolvedClosePressedBackgroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedClosePressedBackgroundProperty =
        ElementBase.Property<NTag, Brush>(nameof(ResolvedClosePressedBackgroundProperty), Brushes.Transparent);

    public double ResolvedAvatarSize
    {
        get => (double)GetValue(ResolvedAvatarSizeProperty);
        private set => SetValue(ResolvedAvatarSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedAvatarSizeProperty =
        ElementBase.Property<NTag, double>(nameof(ResolvedAvatarSizeProperty), 26d);

    public double ResolvedIconSize
    {
        get => (double)GetValue(ResolvedIconSizeProperty);
        private set => SetValue(ResolvedIconSizeProperty, value);
    }

    public static readonly DependencyProperty ResolvedIconSizeProperty =
        ElementBase.Property<NTag, double>(nameof(ResolvedIconSizeProperty), 20d);

    public Thickness ResolvedAvatarMargin
    {
        get => (Thickness)GetValue(ResolvedAvatarMarginProperty);
        private set => SetValue(ResolvedAvatarMarginProperty, value);
    }

    public static readonly DependencyProperty ResolvedAvatarMarginProperty =
        ElementBase.Property<NTag, Thickness>(nameof(ResolvedAvatarMarginProperty), new Thickness(0, 0, 6, 0));

    public Thickness ResolvedIconMargin
    {
        get => (Thickness)GetValue(ResolvedIconMarginProperty);
        private set => SetValue(ResolvedIconMarginProperty, value);
    }

    public static readonly DependencyProperty ResolvedIconMarginProperty =
        ElementBase.Property<NTag, Thickness>(nameof(ResolvedIconMarginProperty), new Thickness(0, 0, 4, 0));

    public static readonly RoutedEvent ClickEvent =
        ElementBase.RoutedEvent<NTag, RoutedEventHandler>(nameof(ClickEvent));

    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    public static readonly RoutedEvent CloseEvent =
        ElementBase.RoutedEvent<NTag, RoutedEventHandler>(nameof(CloseEvent));

    public event RoutedEventHandler Close
    {
        add => AddHandler(CloseEvent, value);
        remove => RemoveHandler(CloseEvent, value);
    }

    public static readonly RoutedEvent CheckedChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(CheckedChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<bool>),
            typeof(NTag));

    public event RoutedPropertyChangedEventHandler<bool> CheckedChanged
    {
        add => AddHandler(CheckedChangedEvent, value);
        remove => RemoveHandler(CheckedChangedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (closeButtonPart is not null)
        {
            closeButtonPart.Click -= HandleCloseButtonClick;
        }

        closeButtonPart = GetTemplateChild(CloseButtonPartName) as ButtonBase;
        if (closeButtonPart is not null)
        {
            closeButtonPart.Click += HandleCloseButtonClick;
        }
    }

    protected override IEnumerator LogicalChildren
    {
        get
        {
            var children = new List<object>();
            var baseChildren = base.LogicalChildren;
            while (baseChildren.MoveNext())
            {
                if (baseChildren.Current is not null)
                {
                    children.Add(baseChildren.Current);
                }
            }

            AppendLogicalChild(children, IconContent);
            AppendLogicalChild(children, AvatarContent);

            return children.GetEnumerator();
        }
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        UpdateResolvedState();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsMouseOverProperty || e.Property == IsEnabledProperty)
        {
            UpdateVisualState();
            return;
        }

        if (e.Property == PaddingProperty)
        {
            UpdateResolvedState();
        }
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);

        if (!CanHandlePointerInteraction(e.OriginalSource as DependencyObject))
        {
            return;
        }

        isPointerPressed = true;
        UpdateVisualState();
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);

        var source = e.OriginalSource as DependencyObject;
        var wasPressed = isPointerPressed;
        isPointerPressed = false;
        UpdateVisualState();

        if (!wasPressed || !CanHandlePointerInteraction(source) || IsWithinCloseButton(source))
        {
            return;
        }

        HandleTagClick();
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        isPointerPressed = false;
        UpdateVisualState();
    }

    private static void OnTagAppearancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NTag tag)
        {
            tag.UpdateResolvedState();
        }
    }

    private static void OnSupplementalContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NTag tag)
        {
            return;
        }

        tag.UpdateLogicalChildRegistration(e.OldValue, e.NewValue);
        tag.UpdateResolvedState();
    }

    private static void OnCheckedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NTag tag)
        {
            return;
        }

        tag.UpdateResolvedState();
        tag.RaiseEvent(new RoutedPropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue, CheckedChangedEvent));
    }

    private static void OnDisabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NTag tag)
        {
            return;
        }

        tag.SyncDisabledState();
        tag.UpdateResolvedState();
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged += HandleThemeChanged;
        SyncDisabledState();
        UpdateResolvedState();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged -= HandleThemeChanged;
    }

    private void HandleThemeChanged(ThemeMode mode)
    {
        UpdateResolvedState();
    }

    private void HandleCloseButtonClick(object sender, RoutedEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        RaiseEvent(new RoutedEventArgs(CloseEvent, this));

        if (TriggerClickOnClose)
        {
            HandleTagClick();
        }
    }

    private void HandleTagClick()
    {
        if (!IsEnabled)
        {
            return;
        }

        if (Checkable)
        {
            Checked = !Checked;
        }

        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
    }

    private void SyncDisabledState()
    {
        if (isSyncingDisabled)
        {
            return;
        }

        try
        {
            isSyncingDisabled = true;
            SetCurrentValue(IsEnabledProperty, !Disabled);
        }
        finally
        {
            isSyncingDisabled = false;
        }
    }

    private void UpdateResolvedState()
    {
        UpdateDisplayContentState();
        UpdateResolvedMetrics();
        ApplyEmbeddedContentOverrides();
        UpdateVisualState();
    }

    private void UpdateDisplayContentState()
    {
        var hasIcon = HasMeaningfulContent(IconContent);
        var hasAvatar = !hasIcon && HasMeaningfulContent(AvatarContent);

        DisplayIconContent = hasIcon ? IconContent : null;
        DisplayAvatarContent = hasAvatar ? AvatarContent : null;
        ShowIcon = hasIcon;
        ShowAvatar = hasAvatar;
        ShowCloseButton = Closable && !Checkable;
        ShowBorder = Bordered && !Checkable;
    }

    private void UpdateResolvedMetrics()
    {
        var (baseHeight, baseFontSize, baseCloseSize, baseCloseIconSize) = Size switch
        {
            NTagSize.Tiny => (16d, 12d, 16d, 10d),
            NTagSize.Small => (22d, 12d, 16d, 10d),
            NTagSize.Large => (34d, 14d, 18d, 12d),
            _ => (28d, 13d, 18d, 12d)
        };

        ResolvedFontSize = double.IsNaN(TextFontSize) ? baseFontSize : TextFontSize;
        var fontDelta = Math.Max(0d, ResolvedFontSize - baseFontSize);
        var hasCustomPadding = HasCustomPadding();
        var metricHeight = baseHeight + fontDelta;

        if (hasCustomPadding)
        {
            metricHeight += Math.Max(0d, Padding.Top) + Math.Max(0d, Padding.Bottom);
        }

        metricHeight = Math.Max(baseHeight, metricHeight);

        ResolvedHeight = metricHeight;
        ResolvedFontWeight = Strong ? FontWeights.SemiBold : FontWeights.Normal;
        var metricScale = baseHeight <= 0d ? 1d : metricHeight / baseHeight;
        ResolvedCloseSize = Math.Max(baseCloseSize, Math.Round(baseCloseSize * metricScale, 0));
        ResolvedCloseIconSize = Math.Max(baseCloseIconSize, Math.Round(baseCloseIconSize * metricScale, 0));
        var affixMetricHeight = AutoScaleIconAndAvatar ? metricHeight : baseHeight;
        ResolvedAvatarSize = Math.Max(affixMetricHeight - 8d, 12d);
        ResolvedIconSize = Math.Max(affixMetricHeight - 8d, 12d);

        if (Round)
        {
            var leftPadding = ShowIcon || ShowAvatar ? metricHeight / 2d : metricHeight / 3d;
            var rightPadding = ShowIcon || ShowAvatar ? metricHeight / 3d : ShowCloseButton ? metricHeight / 4d : metricHeight / 3d;

            ResolvedPadding = ResolvePadding(new Thickness(leftPadding, 0, rightPadding, 0));
            ResolvedCornerRadius = new CornerRadius(metricHeight / 2d);
            ResolvedCloseCornerRadius = new CornerRadius(ResolvedCloseSize / 2d);
            ResolvedIconMargin = new Thickness(-ResolvedIconSize / 2d, 0, 4, 0);
            ResolvedAvatarMargin = new Thickness(-ResolvedAvatarSize / 2d, 0, 6, 0);
            return;
        }

        ResolvedPadding = ResolvePadding(new Thickness(7, 0, 7, 0));
        ResolvedCornerRadius = DefaultCornerRadius;
        ResolvedCloseCornerRadius = new CornerRadius(2d);
        ResolvedIconMargin = new Thickness(0, 0, 4, 0);
        ResolvedAvatarMargin = new Thickness(0, 0, 6, 0);
    }

    private Thickness ResolvePadding(Thickness defaultPadding)
    {
        return HasCustomPadding() ? Padding : defaultPadding;
    }

    private bool HasCustomPadding()
    {
        var valueSource = DependencyPropertyHelper.GetValueSource(this, PaddingProperty);
        return valueSource.BaseValueSource != BaseValueSource.Default;
    }

    private void ApplyEmbeddedContentOverrides()
    {
        if (DisplayAvatarContent is NAvatar avatar)
        {
            if (avatar.ReadLocalValue(NAvatar.SizeProperty) == DependencyProperty.UnsetValue)
            {
                avatar.SetCurrentValue(NAvatar.SizeProperty, ResolvedAvatarSize);
            }

            if (Round)
            {
                avatar.SetCurrentValue(NAvatar.ShapeProperty, NAvatarShape.Round);
            }
        }

        if (DisplayIconContent is NIcon icon && icon.ReadLocalValue(NIcon.SizeProperty) == DependencyProperty.UnsetValue)
        {
            icon.SetCurrentValue(NIcon.SizeProperty, ResolvedIconSize);
        }
    }

    private void UpdateVisualState()
    {
        var isDisabled = !IsEnabled;
        var secondaryColor = GetResourceColor("Theme.Text.Secondary.Color", Colors.Gray);
        var inverseTextColor = GetResourceColor("Theme.Text.Inverse.Color", Colors.White);
        var fill2Color = GetResourceColor("Theme.Fill.2.Color", Colors.Transparent);
        var fill2HoverColor = GetResourceColor("Theme.Fill.2Hover.Color", fill2Color);
        var fill2PressedColor = GetResourceColor("Theme.Fill.2Pressed.Color", fill2HoverColor);
        var hoverColor = GetResourceColor("Theme.Fill.Hover.Color", fill2HoverColor);

        Brush backgroundBrush;
        Brush borderBrush;
        Brush foregroundBrush;
        Brush closeIconBrush;
        Brush closeHoverBackgroundBrush;
        Brush closePressedBackgroundBrush;

        if (Checkable)
        {
            var primaryColor = GetResourceColor("Primary.First.Color", Colors.DodgerBlue);
            var primaryHoverColor = GetResourceColor("Primary.Hover.Color", primaryColor);
            var primaryPressedColor = GetResourceColor("Primary.Pressed.Color", primaryColor);

            borderBrush = Brushes.Transparent;
            closeIconBrush = Brushes.Transparent;
            closeHoverBackgroundBrush = Brushes.Transparent;
            closePressedBackgroundBrush = Brushes.Transparent;

            if (Checked)
            {
                backgroundBrush = CreateBrush(isPointerPressed ? primaryPressedColor : IsMouseOver ? primaryHoverColor : primaryColor);
                foregroundBrush = CreateBrush(inverseTextColor);
            }
            else
            {
                backgroundBrush = CreateBrush(isPointerPressed ? fill2PressedColor : IsMouseOver ? fill2HoverColor : Colors.Transparent);
                foregroundBrush = CreateBrush(secondaryColor);
            }
        }
        else
        {
            ResolveStandardAppearance(
                out backgroundBrush,
                out borderBrush,
                out foregroundBrush,
                out closeIconBrush,
                out closeHoverBackgroundBrush,
                out closePressedBackgroundBrush,
                hoverColor,
                fill2PressedColor);
        }

        ResolvedBackground = backgroundBrush;
        ResolvedBorderBrush = ShowBorder ? borderBrush : Brushes.Transparent;
        ResolvedCloseIconBrush = closeIconBrush;
        ResolvedCloseHoverBackground = closeHoverBackgroundBrush;
        ResolvedClosePressedBackground = closePressedBackgroundBrush;

        SetCurrentValue(ForegroundProperty, foregroundBrush);
        SetCurrentValue(OpacityProperty, isDisabled ? 0.5d : 1d);
        Cursor = !isDisabled && Checkable ? Cursors.Hand : Cursors.Arrow;

        if (DisplayIconContent is NIcon icon
            && icon.ReadLocalValue(NIcon.ColorProperty) == DependencyProperty.UnsetValue
            && icon.ReadLocalValue(NIcon.IconBrushProperty) == DependencyProperty.UnsetValue)
        {
            icon.SetCurrentValue(NIcon.ColorProperty, foregroundBrush);
        }
    }

    private void ResolveStandardAppearance(
        out Brush backgroundBrush,
        out Brush borderBrush,
        out Brush foregroundBrush,
        out Brush closeIconBrush,
        out Brush closeHoverBackgroundBrush,
        out Brush closePressedBackgroundBrush,
        Color defaultCloseHoverColor,
        Color defaultClosePressedColor)
    {
        Color backgroundColor;
        Color borderColor;
        Color foregroundColor;
        Color closeIconColor;
        Color closeHoverBackgroundColor;
        Color closePressedBackgroundColor;

        if (Type == NTagType.Default)
        {
            backgroundColor = ShowBorder
                ? ThemeManager.CurrentTheme == ThemeMode.Dark
                    ? GetResourceColor("Theme.Surface.0.Color", Colors.Black)
                    : System.Windows.Media.Color.FromRgb(250, 250, 252)
                : GetResourceColor("Theme.Fill.2.Color", Colors.Transparent);
            borderColor = GetResourceColor("Theme.Border.Color", Colors.Transparent);
            foregroundColor = GetResourceColor("Theme.Text.Secondary.Color", Colors.Gray);
            closeIconColor = GetResourceColor("Theme.Text.Secondary.Color", Colors.Gray);
            closeHoverBackgroundColor = defaultCloseHoverColor;
            closePressedBackgroundColor = defaultClosePressedColor;
        }
        else
        {
            var typedColor = GetTypeColor(Type);

            backgroundColor = ApplyAlpha(typedColor, ShowBorder ? GetBorderedBackgroundAlpha(Type) : GetFilledBackgroundAlpha(Type));
            borderColor = ApplyAlpha(typedColor, GetBorderAlpha(Type));
            foregroundColor = typedColor;
            closeIconColor = typedColor;
            closeHoverBackgroundColor = ApplyAlpha(typedColor, 0.12d);
            closePressedBackgroundColor = ApplyAlpha(typedColor, 0.18d);
        }

        if (Color is not null)
        {
            if (TryParseColor(Color.Color, out var customBackground))
            {
                backgroundColor = customBackground;
            }

            if (TryParseColor(Color.BorderColor, out var customBorder))
            {
                borderColor = customBorder;
            }

            if (TryParseColor(Color.TextColor, out var customText))
            {
                foregroundColor = customText;
                closeIconColor = customText;
            }
        }

        backgroundBrush = CreateBrush(backgroundColor);
        borderBrush = CreateBrush(borderColor);
        foregroundBrush = CreateBrush(foregroundColor);
        closeIconBrush = CreateBrush(closeIconColor);
        closeHoverBackgroundBrush = CreateBrush(closeHoverBackgroundColor);
        closePressedBackgroundBrush = CreateBrush(closePressedBackgroundColor);
    }

    private bool CanHandlePointerInteraction(DependencyObject? source)
    {
        return IsEnabled
               && !IsInteractiveChildSource(source)
               && !IsWithinCloseButton(source);
    }

    private bool IsWithinCloseButton(DependencyObject? source)
    {
        while (source is not null)
        {
            if (ReferenceEquals(source, closeButtonPart))
            {
                return true;
            }

            source = source switch
            {
                Visual visual => VisualTreeHelper.GetParent(visual),
                System.Windows.Media.Media3D.Visual3D visual3D => VisualTreeHelper.GetParent(visual3D),
                FrameworkContentElement contentElement => contentElement.Parent,
                _ => null
            };
        }

        return false;
    }

    private static bool IsInteractiveChildSource(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is ButtonBase
                || source is TextBoxBase
                || source is ListBoxItem
                || source is ComboBoxItem
                || source is MenuItem)
            {
                return true;
            }

            source = source switch
            {
                Visual visual => VisualTreeHelper.GetParent(visual),
                System.Windows.Media.Media3D.Visual3D visual3D => VisualTreeHelper.GetParent(visual3D),
                FrameworkContentElement contentElement => contentElement.Parent,
                _ => null
            };
        }

        return false;
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

    private static void AppendLogicalChild(ICollection<object> children, object? content)
    {
        if (content is null || children.Contains(content))
        {
            return;
        }

        switch (content)
        {
            case FrameworkElement element when element.Parent is not null:
            case FrameworkContentElement contentElement when contentElement.Parent is not null:
                children.Add(content);
                break;
        }
    }

    private void UpdateLogicalChildRegistration(object? oldContent, object? newContent)
    {
        TryRemoveLogicalChild(oldContent);
        TryAddLogicalChild(newContent);
    }

    private void TryAddLogicalChild(object? content)
    {
        if (!CanManageLogicalChild(content))
        {
            return;
        }

        switch (content)
        {
            case FrameworkElement element when element.Parent is null:
                AddLogicalChild(element);
                break;
            case FrameworkContentElement contentElement when contentElement.Parent is null:
                AddLogicalChild(contentElement);
                break;
        }
    }

    private void TryRemoveLogicalChild(object? content)
    {
        if (!CanManageLogicalChild(content) || IsStillReferenced(content))
        {
            return;
        }

        switch (content)
        {
            case FrameworkElement element when ReferenceEquals(element.Parent, this):
                RemoveLogicalChild(element);
                break;
            case FrameworkContentElement contentElement when ReferenceEquals(contentElement.Parent, this):
                RemoveLogicalChild(contentElement);
                break;
        }
    }

    private bool IsStillReferenced(object? content)
    {
        return ReferenceEquals(content, Content)
               || ReferenceEquals(content, IconContent)
               || ReferenceEquals(content, AvatarContent);
    }

    private static bool CanManageLogicalChild(object? content)
    {
        return content is FrameworkElement or FrameworkContentElement;
    }

    private static double GetFilledBackgroundAlpha(NTagType type)
    {
        return type switch
        {
            NTagType.Warning => 0.15d,
            NTagType.Error => 0.10d,
            _ => 0.12d
        };
    }

    private static double GetBorderedBackgroundAlpha(NTagType type)
    {
        return type switch
        {
            NTagType.Warning => 0.12d,
            NTagType.Error => 0.08d,
            _ => 0.10d
        };
    }

    private static double GetBorderAlpha(NTagType type)
    {
        return type switch
        {
            NTagType.Warning => 0.35d,
            NTagType.Error => 0.23d,
            _ => 0.30d
        };
    }

    private Color GetTypeColor(NTagType type)
    {
        var resourceKey = type switch
        {
            NTagType.Primary => "Primary.First.Color",
            NTagType.Success => "Success.First.Color",
            NTagType.Info => "Info.First.Color",
            NTagType.Warning => "Warning.First.Color",
            NTagType.Error => "Error.First.Color",
            _ => "Theme.Text.Secondary.Color"
        };

        return GetResourceColor(resourceKey, Colors.Gray);
    }

    private Color GetResourceColor(string resourceKey, Color fallback)
    {
        if (TryFindResource(resourceKey) is Color directColor)
        {
            return directColor;
        }

        if (TryFindResource(resourceKey) is SolidColorBrush brush)
        {
            return brush.Color;
        }

        if (Application.Current?.TryFindResource(resourceKey) is Color appColor)
        {
            return appColor;
        }

        if (Application.Current?.TryFindResource(resourceKey) is SolidColorBrush appBrush)
        {
            return appBrush.Color;
        }

        return fallback;
    }

    private static bool TryParseColor(string? text, out Color color)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            try
            {
                var converted = ColorConverter.ConvertFromString(text.Trim());
                if (converted is Color parsedColor)
                {
                    color = parsedColor;
                    return true;
                }
            }
            catch
            {
            }
        }

        color = default;
        return false;
    }

    private static Color ApplyAlpha(Color color, double alpha)
    {
        var clamped = Math.Clamp(alpha, 0d, 1d);
        return System.Windows.Media.Color.FromArgb((byte)Math.Round(clamped * 255d), color.R, color.G, color.B);
    }

    private static SolidColorBrush CreateBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
