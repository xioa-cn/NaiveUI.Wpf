using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
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
    Primary
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

public class NFloatButton : Button
{
    private const string MenuPopupPartName = "PART_MenuPopup";
    private const string MenuContentHostPartName = "PART_MenuContentHost";
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
        ElementBase.Property<NFloatButton, NFloatButtonType>(nameof(TypeProperty), NFloatButtonType.Default);

    public NFloatButtonMenuTrigger MenuTrigger
    {
        get => (NFloatButtonMenuTrigger)GetValue(MenuTriggerProperty);
        set => SetValue(MenuTriggerProperty, value);
    }

    public static readonly DependencyProperty MenuTriggerProperty =
        ElementBase.Property<NFloatButton, NFloatButtonMenuTrigger>(nameof(MenuTriggerProperty), NFloatButtonMenuTrigger.None, OnMenuPropertyChanged);

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
    }

    internal void SetGroupContext(bool grouped, NFloatButtonShape shape, bool isLastItem)
    {
        isGrouped = grouped;
        groupedShape = shape;
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
        userMargin = Margin;
        userHorizontalAlignment = HorizontalAlignment;
        userVerticalAlignment = VerticalAlignment;

        UpdateResolvedState();
        UpdateLayoutState();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        hoverCloseTimer.Stop();
        DetachPopupHandlers();
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
        ResolvedCornerRadius = ResolvedShape == NFloatButtonShape.Circle ? CircleCornerRadius : SquareCornerRadius;
        UseSquareGroupChrome = isGrouped && ResolvedShape == NFloatButtonShape.Square;
        ShowGroupSeparator = UseSquareGroupChrome && !isLastGroupedItem;

        if (!HasMenu && ShowMenu)
        {
            SetCurrentValue(ShowMenuProperty, false);
        }
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
        menuPopupPart.VerticalOffset = -8d;
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
}
