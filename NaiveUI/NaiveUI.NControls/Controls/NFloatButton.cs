using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using NaiveUI.NControls.Themes;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NFloatButtonShape
{
    Circle,
    Square
}

public enum NFloatButtonType
{
    Default,
    Primary,
    Info,
    Success,
    Warning,
    Error,
    Customize
}

public enum NFloatButtonPosition
{
    Relative,
    Absolute,
    Fixed
}

public enum NFloatButtonMenuTrigger
{
    None,
    Hover,
    Click
}

public enum NFloatButtonMenuPlacement
{
    Top,
    Right,
    Bottom,
    Left
}

public class NFloatButton : Button
{
    private const string MenuPopupPartName = "PART_MenuPopup";
    private const string MenuContentHostPartName = "PART_MenuContentHost";
    private const double MenuPopupGap = 8d;
    private static readonly CornerRadius CircleCornerRadius = new(999d);
    private static readonly CornerRadius SquareCornerRadius = new(8d);
    private readonly DispatcherTimer hoverCloseTimer;
    private Popup? menuPopupPart;
    private ContentControl? menuContentHostPart;
    private FrameworkElement? popupChildPart;
    private bool applyingAutoLayout;
    private Thickness userMargin;
    private HorizontalAlignment userHorizontalAlignment;
    private VerticalAlignment userVerticalAlignment;
    private bool isGrouped;
    private NFloatButtonShape groupedShape;
    private Orientation groupedOrientation;
    private bool isLastGroupedItem;

    static NFloatButton()
    {
        ElementBase.DefaultStyle<NFloatButton>(DefaultStyleKeyProperty);
    }

    public NFloatButton()
    {
        hoverCloseTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(120)
        };
        hoverCloseTimer.Tick += HandleHoverCloseTimerTick;

        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;

        userMargin = Margin;
        userHorizontalAlignment = HorizontalAlignment;
        userVerticalAlignment = VerticalAlignment;

        UpdateResolvedState();
        UpdateResolvedBrushes();
    }

    public object? Left
    {
        get => GetValue(LeftProperty);
        set => SetValue(LeftProperty, value);
    }

    public static readonly DependencyProperty LeftProperty =
        ElementBase.Property<NFloatButton, object?>(nameof(LeftProperty), null, OnLayoutPropertyChanged);

    public object? Right
    {
        get => GetValue(RightProperty);
        set => SetValue(RightProperty, value);
    }

    public static readonly DependencyProperty RightProperty =
        ElementBase.Property<NFloatButton, object?>(nameof(RightProperty), null, OnLayoutPropertyChanged);

    public object? Top
    {
        get => GetValue(TopProperty);
        set => SetValue(TopProperty, value);
    }

    public static readonly DependencyProperty TopProperty =
        ElementBase.Property<NFloatButton, object?>(nameof(TopProperty), null, OnLayoutPropertyChanged);

    public object? Bottom
    {
        get => GetValue(BottomProperty);
        set => SetValue(BottomProperty, value);
    }

    public static readonly DependencyProperty BottomProperty =
        ElementBase.Property<NFloatButton, object?>(nameof(BottomProperty), null, OnLayoutPropertyChanged);

    public NFloatButtonShape Shape
    {
        get => (NFloatButtonShape)GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public static readonly DependencyProperty ShapeProperty =
        ElementBase.Property<NFloatButton, NFloatButtonShape>(nameof(ShapeProperty), NFloatButtonShape.Circle, OnAppearancePropertyChanged);

    public NFloatButtonPosition Position
    {
        get => (NFloatButtonPosition)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public static readonly DependencyProperty PositionProperty =
        ElementBase.Property<NFloatButton, NFloatButtonPosition>(nameof(PositionProperty), NFloatButtonPosition.Fixed, OnLayoutPropertyChanged);

    public NFloatButtonType Type
    {
        get => (NFloatButtonType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public static readonly DependencyProperty TypeProperty =
        ElementBase.Property<NFloatButton, NFloatButtonType>(nameof(TypeProperty), NFloatButtonType.Default, OnAppearancePropertyChanged);

    public NFloatButtonMenuTrigger MenuTrigger
    {
        get => (NFloatButtonMenuTrigger)GetValue(MenuTriggerProperty);
        set => SetValue(MenuTriggerProperty, value);
    }

    public static readonly DependencyProperty MenuTriggerProperty =
        ElementBase.Property<NFloatButton, NFloatButtonMenuTrigger>(nameof(MenuTriggerProperty), NFloatButtonMenuTrigger.None, OnMenuPropertyChanged);

    public NFloatButtonMenuPlacement MenuPlacement
    {
        get => (NFloatButtonMenuPlacement)GetValue(MenuPlacementProperty);
        set => SetValue(MenuPlacementProperty, value);
    }

    public static readonly DependencyProperty MenuPlacementProperty =
        ElementBase.Property<NFloatButton, NFloatButtonMenuPlacement>(nameof(MenuPlacementProperty), NFloatButtonMenuPlacement.Top, OnMenuPlacementPropertyChanged);

    public Orientation MenuOrientation
    {
        get => (Orientation)GetValue(MenuOrientationProperty);
        set => SetValue(MenuOrientationProperty, value);
    }

    public static readonly DependencyProperty MenuOrientationProperty =
        ElementBase.Property<NFloatButton, Orientation>(nameof(MenuOrientationProperty), Orientation.Vertical, OnMenuLayoutPropertyChanged);

    public bool ShowMenu
    {
        get => (bool)GetValue(ShowMenuProperty);
        set => SetValue(ShowMenuProperty, value);
    }

    public static readonly DependencyProperty ShowMenuProperty =
        DependencyProperty.Register(
            nameof(ShowMenu),
            typeof(bool),
            typeof(NFloatButton),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnShowMenuPropertyChanged));

    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        ElementBase.Property<NFloatButton, object?>(nameof(DescriptionProperty), null, OnAppearancePropertyChanged);

    public object? Menu
    {
        get => GetValue(MenuProperty);
        set => SetValue(MenuProperty, value);
    }

    public static readonly DependencyProperty MenuProperty =
        ElementBase.Property<NFloatButton, object?>(nameof(MenuProperty), null, OnMenuPropertyChanged);

    public bool HasDescription
    {
        get => (bool)GetValue(HasDescriptionProperty);
        private set => SetValue(HasDescriptionProperty, value);
    }

    public static readonly DependencyProperty HasDescriptionProperty =
        ElementBase.Property<NFloatButton, bool>(nameof(HasDescriptionProperty), false);

    public bool HasMenu
    {
        get => (bool)GetValue(HasMenuProperty);
        private set => SetValue(HasMenuProperty, value);
    }

    public static readonly DependencyProperty HasMenuProperty =
        ElementBase.Property<NFloatButton, bool>(nameof(HasMenuProperty), false);

    public NFloatButtonShape ResolvedShape
    {
        get => (NFloatButtonShape)GetValue(ResolvedShapeProperty);
        private set => SetValue(ResolvedShapeProperty, value);
    }

    public static readonly DependencyProperty ResolvedShapeProperty =
        ElementBase.Property<NFloatButton, NFloatButtonShape>(nameof(ResolvedShapeProperty), NFloatButtonShape.Circle);

    public CornerRadius ResolvedCornerRadius
    {
        get => (CornerRadius)GetValue(ResolvedCornerRadiusProperty);
        private set => SetValue(ResolvedCornerRadiusProperty, value);
    }

    public static readonly DependencyProperty ResolvedCornerRadiusProperty =
        ElementBase.Property<NFloatButton, CornerRadius>(nameof(ResolvedCornerRadiusProperty), CircleCornerRadius);

    public bool UseSquareGroupChrome
    {
        get => (bool)GetValue(UseSquareGroupChromeProperty);
        private set => SetValue(UseSquareGroupChromeProperty, value);
    }

    public static readonly DependencyProperty UseSquareGroupChromeProperty =
        ElementBase.Property<NFloatButton, bool>(nameof(UseSquareGroupChromeProperty), false);

    public bool ShowGroupSeparator
    {
        get => (bool)GetValue(ShowGroupSeparatorProperty);
        private set => SetValue(ShowGroupSeparatorProperty, value);
    }

    public static readonly DependencyProperty ShowGroupSeparatorProperty =
        ElementBase.Property<NFloatButton, bool>(nameof(ShowGroupSeparatorProperty), false);

    public Orientation ResolvedGroupOrientation
    {
        get => (Orientation)GetValue(ResolvedGroupOrientationProperty);
        private set => SetValue(ResolvedGroupOrientationProperty, value);
    }

    public static readonly DependencyProperty ResolvedGroupOrientationProperty =
        ElementBase.Property<NFloatButton, Orientation>(nameof(ResolvedGroupOrientationProperty), Orientation.Vertical);

    public Brush? CustomBackgroundBrush
    {
        get => (Brush?)GetValue(CustomBackgroundBrushProperty);
        set => SetValue(CustomBackgroundBrushProperty, value);
    }

    public static readonly DependencyProperty CustomBackgroundBrushProperty =
        ElementBase.Property<NFloatButton, Brush?>(nameof(CustomBackgroundBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? CustomForegroundBrush
    {
        get => (Brush?)GetValue(CustomForegroundBrushProperty);
        set => SetValue(CustomForegroundBrushProperty, value);
    }

    public static readonly DependencyProperty CustomForegroundBrushProperty =
        ElementBase.Property<NFloatButton, Brush?>(nameof(CustomForegroundBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? CustomHoverBackgroundBrush
    {
        get => (Brush?)GetValue(CustomHoverBackgroundBrushProperty);
        set => SetValue(CustomHoverBackgroundBrushProperty, value);
    }

    public static readonly DependencyProperty CustomHoverBackgroundBrushProperty =
        ElementBase.Property<NFloatButton, Brush?>(nameof(CustomHoverBackgroundBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? CustomHoverForegroundBrush
    {
        get => (Brush?)GetValue(CustomHoverForegroundBrushProperty);
        set => SetValue(CustomHoverForegroundBrushProperty, value);
    }

    public static readonly DependencyProperty CustomHoverForegroundBrushProperty =
        ElementBase.Property<NFloatButton, Brush?>(nameof(CustomHoverForegroundBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? CustomPressedBackgroundBrush
    {
        get => (Brush?)GetValue(CustomPressedBackgroundBrushProperty);
        set => SetValue(CustomPressedBackgroundBrushProperty, value);
    }

    public static readonly DependencyProperty CustomPressedBackgroundBrushProperty =
        ElementBase.Property<NFloatButton, Brush?>(nameof(CustomPressedBackgroundBrushProperty), null, OnAppearancePropertyChanged);

    public Brush? CustomPressedForegroundBrush
    {
        get => (Brush?)GetValue(CustomPressedForegroundBrushProperty);
        set => SetValue(CustomPressedForegroundBrushProperty, value);
    }

    public static readonly DependencyProperty CustomPressedForegroundBrushProperty =
        ElementBase.Property<NFloatButton, Brush?>(nameof(CustomPressedForegroundBrushProperty), null, OnAppearancePropertyChanged);

    public Brush ResolvedBackground
    {
        get => (Brush)GetValue(ResolvedBackgroundProperty);
        private set => SetValue(ResolvedBackgroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedBackgroundProperty =
        ElementBase.Property<NFloatButton, Brush>(nameof(ResolvedBackgroundProperty), Brushes.Transparent);

    public Brush ResolvedForeground
    {
        get => (Brush)GetValue(ResolvedForegroundProperty);
        private set => SetValue(ResolvedForegroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedForegroundProperty =
        ElementBase.Property<NFloatButton, Brush>(nameof(ResolvedForegroundProperty), Brushes.Black);

    public Brush ResolvedHoverBackground
    {
        get => (Brush)GetValue(ResolvedHoverBackgroundProperty);
        private set => SetValue(ResolvedHoverBackgroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedHoverBackgroundProperty =
        ElementBase.Property<NFloatButton, Brush>(nameof(ResolvedHoverBackgroundProperty), Brushes.Transparent);

    public Brush ResolvedHoverForeground
    {
        get => (Brush)GetValue(ResolvedHoverForegroundProperty);
        private set => SetValue(ResolvedHoverForegroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedHoverForegroundProperty =
        ElementBase.Property<NFloatButton, Brush>(nameof(ResolvedHoverForegroundProperty), Brushes.Black);

    public Brush ResolvedPressedBackground
    {
        get => (Brush)GetValue(ResolvedPressedBackgroundProperty);
        private set => SetValue(ResolvedPressedBackgroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedPressedBackgroundProperty =
        ElementBase.Property<NFloatButton, Brush>(nameof(ResolvedPressedBackgroundProperty), Brushes.Transparent);

    public Brush ResolvedPressedForeground
    {
        get => (Brush)GetValue(ResolvedPressedForegroundProperty);
        private set => SetValue(ResolvedPressedForegroundProperty, value);
    }

    public static readonly DependencyProperty ResolvedPressedForegroundProperty =
        ElementBase.Property<NFloatButton, Brush>(nameof(ResolvedPressedForegroundProperty), Brushes.Black);

    public override void OnApplyTemplate()
    {
        DetachPopupHandlers();

        base.OnApplyTemplate();

        menuPopupPart = GetTemplateChild(MenuPopupPartName) as Popup;
        menuContentHostPart = GetTemplateChild(MenuContentHostPartName) as ContentControl;
        popupChildPart = menuPopupPart?.Child as FrameworkElement;

        UpdateMenuHostContent();
        AttachPopupHandlers();
        SyncPopupState();
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        hoverCloseTimer.Stop();

        if (HasMenu && MenuTrigger == NFloatButtonMenuTrigger.Hover)
        {
            SetCurrentValue(ShowMenuProperty, true);
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        if (HasMenu && MenuTrigger == NFloatButtonMenuTrigger.Hover)
        {
            StartHoverCloseTimer();
        }
    }

    protected override void OnClick()
    {
        if (HasMenu)
        {
            if (MenuTrigger == NFloatButtonMenuTrigger.Click)
            {
                SetCurrentValue(ShowMenuProperty, !ShowMenu);
            }

            return;
        }

        base.OnClick();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (HasMenu && e.Key == Key.Escape && ShowMenu)
        {
            SetCurrentValue(ShowMenuProperty, false);
            e.Handled = true;
            return;
        }

        base.OnPreviewKeyDown(e);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (applyingAutoLayout)
        {
            return;
        }

        if (e.Property == MarginProperty)
        {
            userMargin = (Thickness)e.NewValue;
            UpdateLayoutState();
        }
        else if (e.Property == HorizontalAlignmentProperty)
        {
            userHorizontalAlignment = (HorizontalAlignment)e.NewValue;
        }
        else if (e.Property == VerticalAlignmentProperty)
        {
            userVerticalAlignment = (VerticalAlignment)e.NewValue;
        }
        else if (e.Property == IsMouseOverProperty || e.Property == IsPressedProperty || e.Property == IsEnabledProperty)
        {
            UpdateVisualState();
        }
    }

    internal void SetGroupContext(bool grouped, NFloatButtonShape shape, Orientation orientation, bool isLastItem)
    {
        isGrouped = grouped;
        groupedShape = shape;
        groupedOrientation = orientation;
        isLastGroupedItem = isLastItem;

        UpdateResolvedState();
        UpdateLayoutState();
    }

    private static void OnAppearancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NFloatButton button)
        {
            return;
        }

        button.UpdateResolvedState();
        button.UpdateResolvedBrushes();
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NFloatButton button)
        {
            return;
        }

        button.UpdateLayoutState();
    }

    private static void OnMenuPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NFloatButton button)
        {
            return;
        }

        button.UpdateResolvedState();
        button.UpdateMenuHostContent();
        button.SyncPopupState();
    }

    private static void OnMenuPlacementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NFloatButton button)
        {
            return;
        }

        button.SyncPopupState();
    }

    private static void OnMenuLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NFloatButton button)
        {
            return;
        }

        button.ApplyMenuLayout();
    }

    private static void OnShowMenuPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NFloatButton button)
        {
            return;
        }

        button.SyncPopupState();
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged += HandleThemeChanged;
        userMargin = Margin;
        userHorizontalAlignment = HorizontalAlignment;
        userVerticalAlignment = VerticalAlignment;

        UpdateResolvedState();
        UpdateResolvedBrushes();
        UpdateLayoutState();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        hoverCloseTimer.Stop();
        DetachPopupHandlers();
        ThemeManager.ThemeChanged -= HandleThemeChanged;
    }

    private void HandleThemeChanged(ThemeMode mode)
    {
        UpdateResolvedBrushes();
    }

    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        if (ShowMenu)
        {
            SetCurrentValue(ShowMenuProperty, false);
        }
    }

    private void HandlePopupChildMouseEnter(object sender, MouseEventArgs e)
    {
        hoverCloseTimer.Stop();
    }

    private void HandlePopupChildMouseLeave(object sender, MouseEventArgs e)
    {
        if (HasMenu && MenuTrigger == NFloatButtonMenuTrigger.Hover)
        {
            StartHoverCloseTimer();
        }
    }

    private void HandleHoverCloseTimerTick(object? sender, EventArgs e)
    {
        hoverCloseTimer.Stop();

        if (HasMenu && MenuTrigger == NFloatButtonMenuTrigger.Hover)
        {
            SetCurrentValue(ShowMenuProperty, false);
        }
    }

    private void StartHoverCloseTimer()
    {
        hoverCloseTimer.Stop();
        hoverCloseTimer.Start();
    }

    private void UpdateResolvedState()
    {
        HasDescription = Description is not null;
        HasMenu = Menu is not null && MenuTrigger != NFloatButtonMenuTrigger.None;
        ResolvedShape = isGrouped ? groupedShape : Shape;
        ResolvedGroupOrientation = isGrouped ? groupedOrientation : Orientation.Vertical;
        ResolvedCornerRadius = ResolvedShape == NFloatButtonShape.Circle ? CircleCornerRadius : SquareCornerRadius;
        UseSquareGroupChrome = isGrouped && ResolvedShape == NFloatButtonShape.Square;
        ShowGroupSeparator = UseSquareGroupChrome && !isLastGroupedItem;

        if (!HasMenu && ShowMenu)
        {
            SetCurrentValue(ShowMenuProperty, false);
        }
    }

    private void UpdateResolvedBrushes()
    {
        if (HasCustomAppearance())
        {
            SetResolved(
                CustomBackgroundBrush ?? GetBrush("Theme.Surface.0.Brush"),
                CustomForegroundBrush ?? GetBrush("Theme.Text.Primary.Brush"),
                CustomHoverBackgroundBrush ?? CustomBackgroundBrush ?? GetBrush("Theme.Fill.2Hover.Brush"),
                CustomHoverForegroundBrush ?? CustomForegroundBrush ?? GetBrush("Theme.Text.Primary.Brush"),
                CustomPressedBackgroundBrush ?? CustomHoverBackgroundBrush ?? CustomBackgroundBrush ?? GetBrush("Theme.Fill.2Pressed.Brush"),
                CustomPressedForegroundBrush ?? CustomHoverForegroundBrush ?? CustomForegroundBrush ?? GetBrush("Theme.Text.Primary.Brush"));
            UpdateVisualState();
            return;
        }

        var tonePrefix = Type switch
        {
            NFloatButtonType.Primary => "Primary",
            NFloatButtonType.Info => "Info",
            NFloatButtonType.Success => "Success",
            NFloatButtonType.Warning => "Warning",
            NFloatButtonType.Error => "Error",
            _ => "Default"
        };

        if (Type == NFloatButtonType.Default)
        {
            SetResolved(
                "Theme.Surface.0.Brush",
                "Theme.Text.Primary.Brush",
                "Theme.Fill.2Hover.Brush",
                "Theme.Text.Primary.Brush",
                "Theme.Fill.2Pressed.Brush",
                "Theme.Text.Primary.Brush");
        }
        else
        {
            SetResolved(
                $"{tonePrefix}.First.Brush",
                "Theme.Text.Inverse.Brush",
                $"{tonePrefix}.Hover.Brush",
                "Theme.Text.Inverse.Brush",
                $"{tonePrefix}.Pressed.Brush",
                "Theme.Text.Inverse.Brush");
        }

        UpdateVisualState();
    }

    private void UpdateLayoutState()
    {
        if (!IsLoaded)
        {
            return;
        }

        if (isGrouped || Position == NFloatButtonPosition.Relative)
        {
            ApplyLayoutState(userMargin, userHorizontalAlignment, userVerticalAlignment);
            return;
        }

        var margin = userMargin;
        var horizontalAlignment = userHorizontalAlignment;
        var verticalAlignment = userVerticalAlignment;

        if (TryResolveOffset(Left, out var left))
        {
            horizontalAlignment = HorizontalAlignment.Left;
            margin.Left = userMargin.Left + left;
        }
        else if (TryResolveOffset(Right, out var right))
        {
            horizontalAlignment = HorizontalAlignment.Right;
            margin.Right = userMargin.Right + right;
        }

        if (TryResolveOffset(Top, out var top))
        {
            verticalAlignment = VerticalAlignment.Top;
            margin.Top = userMargin.Top + top;
        }
        else if (TryResolveOffset(Bottom, out var bottom))
        {
            verticalAlignment = VerticalAlignment.Bottom;
            margin.Bottom = userMargin.Bottom + bottom;
        }

        ApplyLayoutState(margin, horizontalAlignment, verticalAlignment);
    }

    private void ApplyLayoutState(Thickness margin, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
    {
        applyingAutoLayout = true;

        try
        {
            SetCurrentValue(MarginProperty, margin);
            SetCurrentValue(HorizontalAlignmentProperty, horizontalAlignment);
            SetCurrentValue(VerticalAlignmentProperty, verticalAlignment);
        }
        finally
        {
            applyingAutoLayout = false;
        }
    }

    private void AttachPopupHandlers()
    {
        if (menuPopupPart is not null)
        {
            menuPopupPart.Closed += HandlePopupClosed;
        }

        if (popupChildPart is not null)
        {
            popupChildPart.MouseEnter += HandlePopupChildMouseEnter;
            popupChildPart.MouseLeave += HandlePopupChildMouseLeave;
        }
    }

    private void DetachPopupHandlers()
    {
        if (menuPopupPart is not null)
        {
            menuPopupPart.Closed -= HandlePopupClosed;
        }

        if (popupChildPart is not null)
        {
            popupChildPart.MouseEnter -= HandlePopupChildMouseEnter;
            popupChildPart.MouseLeave -= HandlePopupChildMouseLeave;
        }

        menuPopupPart = null;
        menuContentHostPart = null;
        popupChildPart = null;
    }

    private void SyncPopupState()
    {
        if (menuPopupPart is null)
        {
            return;
        }

        menuPopupPart.PlacementTarget = this;
        menuPopupPart.Placement = PlacementMode.Custom;
        menuPopupPart.CustomPopupPlacementCallback = HandleMenuPopupPlacement;
        menuPopupPart.StaysOpen = MenuTrigger != NFloatButtonMenuTrigger.Click;
        menuPopupPart.IsOpen = HasMenu && ShowMenu;
    }

    private void UpdateMenuHostContent()
    {
        if (menuContentHostPart is null)
        {
            return;
        }

        menuContentHostPart.Content = Menu;
        ApplyMenuLayout();
    }

    internal static bool TryResolveOffset(object? value, out double offset)
    {
        switch (value)
        {
            case null:
                break;
            case double doubleValue when !double.IsNaN(doubleValue):
                offset = doubleValue;
                return true;
            case float floatValue when !float.IsNaN(floatValue):
                offset = floatValue;
                return true;
            case int intValue:
                offset = intValue;
                return true;
            case long longValue:
                offset = longValue;
                return true;
            case string text:
            {
                var normalized = text.Trim();
                if (normalized.EndsWith("px", StringComparison.OrdinalIgnoreCase))
                {
                    normalized = normalized[..^2].Trim();
                }

                if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedOffset))
                {
                    offset = parsedOffset;
                    return true;
                }

                break;
            }
        }

        offset = 0d;
        return false;
    }

    private CustomPopupPlacement[] HandleMenuPopupPlacement(Size popupSize, Size targetSize, Point offset)
    {
        return MenuPlacement switch
        {
            NFloatButtonMenuPlacement.Top => [new CustomPopupPlacement(new Point((targetSize.Width - popupSize.Width) / 2d, -popupSize.Height - MenuPopupGap), PopupPrimaryAxis.Vertical)],
            NFloatButtonMenuPlacement.Right => [new CustomPopupPlacement(new Point(targetSize.Width + MenuPopupGap, (targetSize.Height - popupSize.Height) / 2d), PopupPrimaryAxis.Horizontal)],
            NFloatButtonMenuPlacement.Bottom => [new CustomPopupPlacement(new Point((targetSize.Width - popupSize.Width) / 2d, targetSize.Height + MenuPopupGap), PopupPrimaryAxis.Vertical)],
            NFloatButtonMenuPlacement.Left => [new CustomPopupPlacement(new Point(-popupSize.Width - MenuPopupGap, (targetSize.Height - popupSize.Height) / 2d), PopupPrimaryAxis.Horizontal)],
            _ => [new CustomPopupPlacement(new Point((targetSize.Width - popupSize.Width) / 2d, -popupSize.Height - MenuPopupGap), PopupPrimaryAxis.Vertical)]
        };
    }

    private void ApplyMenuLayout()
    {
        ApplyMenuLayoutToContent(Menu);
    }

    private void ApplyMenuLayoutToContent(object? content)
    {
        switch (content)
        {
            case NFloatButtonGroup group:
                TryApplyOrientation(group, NFloatButtonGroup.OrientationProperty);
                break;
            case NFloatButtonStackPanel floatPanel:
                TryApplyOrientation(floatPanel, NFloatButtonStackPanel.OrientationProperty);
                break;
            case StackPanel stackPanel:
                TryApplyOrientation(stackPanel, StackPanel.OrientationProperty);
                break;
            case WrapPanel wrapPanel:
                TryApplyOrientation(wrapPanel, WrapPanel.OrientationProperty);
                break;
        }
    }

    private void TryApplyOrientation(DependencyObject target, DependencyProperty property)
    {
        if (target.ReadLocalValue(property) != DependencyProperty.UnsetValue)
        {
            return;
        }

        target.SetValue(property, MenuOrientation);
    }

    private bool HasCustomAppearance()
    {
        return Type == NFloatButtonType.Customize
               || CustomBackgroundBrush is not null
               || CustomForegroundBrush is not null
               || CustomHoverBackgroundBrush is not null
               || CustomHoverForegroundBrush is not null
               || CustomPressedBackgroundBrush is not null
               || CustomPressedForegroundBrush is not null;
    }

    private void SetResolved(
        string backgroundKey,
        string foregroundKey,
        string hoverBackgroundKey,
        string hoverForegroundKey,
        string pressedBackgroundKey,
        string pressedForegroundKey)
    {
        SetResolved(
            GetBrush(backgroundKey),
            GetBrush(foregroundKey),
            GetBrush(hoverBackgroundKey),
            GetBrush(hoverForegroundKey),
            GetBrush(pressedBackgroundKey),
            GetBrush(pressedForegroundKey));
    }

    private void SetResolved(
        Brush background,
        Brush foreground,
        Brush hoverBackground,
        Brush hoverForeground,
        Brush pressedBackground,
        Brush pressedForeground)
    {
        ResolvedBackground = background;
        ResolvedForeground = foreground;
        ResolvedHoverBackground = hoverBackground;
        ResolvedHoverForeground = hoverForeground;
        ResolvedPressedBackground = pressedBackground;
        ResolvedPressedForeground = pressedForeground;
    }

    private void UpdateVisualState()
    {
        if (!IsEnabled)
        {
            Background = ResolvedBackground;
            Foreground = ResolvedForeground;
            return;
        }

        if (IsPressed)
        {
            Background = ResolvedPressedBackground;
            Foreground = ResolvedPressedForeground;
            return;
        }

        if (IsMouseOver)
        {
            Background = ResolvedHoverBackground;
            Foreground = ResolvedHoverForeground;
            return;
        }

        Background = ResolvedBackground;
        Foreground = ResolvedForeground;
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
