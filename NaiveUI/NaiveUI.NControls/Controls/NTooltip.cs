using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public enum NTooltipTrigger
{
    Hover,
    Click,
    Focus,
    Manual
}

public enum NTooltipPlacement
{
    Top,
    TopStart,
    TopEnd,
    Right,
    RightStart,
    RightEnd,
    Bottom,
    BottomStart,
    BottomEnd,
    Left,
    LeftStart,
    LeftEnd
}

public sealed class NTooltipPopup : ContentControl
{
    static NTooltipPopup()
    {
        ElementBase.DefaultStyle<NTooltipPopup>(DefaultStyleKeyProperty);
    }

    public bool ShowArrowVisual
    {
        get => (bool)GetValue(ShowArrowVisualProperty);
        internal set => SetValue(ShowArrowVisualProperty, value);
    }

    public static readonly DependencyProperty ShowArrowVisualProperty =
        ElementBase.Property<NTooltipPopup, bool>(nameof(ShowArrowVisualProperty), false);

    public Thickness PopupContentMargin
    {
        get => (Thickness)GetValue(PopupContentMarginProperty);
        internal set => SetValue(PopupContentMarginProperty, value);
    }

    public static readonly DependencyProperty PopupContentMarginProperty =
        ElementBase.Property<NTooltipPopup, Thickness>(nameof(PopupContentMarginProperty), new Thickness(0));

    public double ArrowLeft
    {
        get => (double)GetValue(ArrowLeftProperty);
        internal set => SetValue(ArrowLeftProperty, value);
    }

    public static readonly DependencyProperty ArrowLeftProperty =
        ElementBase.Property<NTooltipPopup, double>(nameof(ArrowLeftProperty), 0d);

    public double ArrowTop
    {
        get => (double)GetValue(ArrowTopProperty);
        internal set => SetValue(ArrowTopProperty, value);
    }

    public static readonly DependencyProperty ArrowTopProperty =
        ElementBase.Property<NTooltipPopup, double>(nameof(ArrowTopProperty), 0d);

    public double ArrowWidth
    {
        get => (double)GetValue(ArrowWidthProperty);
        internal set => SetValue(ArrowWidthProperty, value);
    }

    public static readonly DependencyProperty ArrowWidthProperty =
        ElementBase.Property<NTooltipPopup, double>(nameof(ArrowWidthProperty), 0d);

    public double ArrowHeight
    {
        get => (double)GetValue(ArrowHeightProperty);
        internal set => SetValue(ArrowHeightProperty, value);
    }

    public static readonly DependencyProperty ArrowHeightProperty =
        ElementBase.Property<NTooltipPopup, double>(nameof(ArrowHeightProperty), 0d);

    public Geometry ArrowData
    {
        get => (Geometry)GetValue(ArrowDataProperty);
        internal set => SetValue(ArrowDataProperty, value);
    }

    public static readonly DependencyProperty ArrowDataProperty =
        ElementBase.Property<NTooltipPopup, Geometry>(nameof(ArrowDataProperty), Geometry.Empty);

    public double ArrowSize
    {
        get => (double)GetValue(ArrowSizeProperty);
        set => SetValue(ArrowSizeProperty, value);
    }

    public static readonly DependencyProperty ArrowSizeProperty =
        ElementBase.Property<NTooltipPopup, double>(nameof(ArrowSizeProperty), 8d);

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        ElementBase.Property<NTooltipPopup, CornerRadius>(nameof(CornerRadiusProperty), new CornerRadius(6));

    public bool Raw
    {
        get => (bool)GetValue(RawProperty);
        set => SetValue(RawProperty, value);
    }

    public static readonly DependencyProperty RawProperty =
        ElementBase.Property<NTooltipPopup, bool>(nameof(RawProperty), false);

    internal void UpdateChromeLayout(
        bool showArrow,
        Thickness contentMargin,
        double arrowLeft,
        double arrowTop,
        double arrowWidth,
        double arrowHeight,
        Geometry arrowData)
    {
        ShowArrowVisual = showArrow;
        PopupContentMargin = contentMargin;
        ArrowLeft = arrowLeft;
        ArrowTop = arrowTop;
        ArrowWidth = arrowWidth;
        ArrowHeight = arrowHeight;
        ArrowData = arrowData;
    }
}

public class NTooltip : ContentControl
{
    private static readonly Geometry ArrowDownGeometry = CreateFrozenGeometry("M 0,0 L 0.5,1 L 1,0 Z");
    private static readonly Geometry ArrowUpGeometry = CreateFrozenGeometry("M 0,1 L 0.5,0 L 1,1 Z");
    private static readonly Geometry ArrowLeftGeometry = CreateFrozenGeometry("M 1,0 L 0,0.5 L 1,1 Z");
    private static readonly Geometry ArrowRightGeometry = CreateFrozenGeometry("M 0,0 L 1,0.5 L 0,1 Z");

    private readonly DispatcherTimer closeTimer;
    private readonly DispatcherTimer openTimer;
    private readonly Popup popup;
    private readonly NTooltipPopup popupChrome;
    private Window? ownerWindow;
    private bool synchronizingShow;

    static NTooltip()
    {
        ElementBase.DefaultStyle<NTooltip>(DefaultStyleKeyProperty);
    }

    public NTooltip()
    {
        popupChrome = new NTooltipPopup();
        popupChrome.SetBinding(ContentProperty, new Binding(nameof(TooltipContent)) { Source = this });
        popupChrome.SetBinding(FrameworkElement.DataContextProperty, new Binding(nameof(DataContext)) { Source = this });
        popupChrome.SetBinding(Control.BackgroundProperty, new Binding(nameof(Background)) { Source = this });
        popupChrome.SetBinding(Control.ForegroundProperty, new Binding(nameof(Foreground)) { Source = this });
        popupChrome.SetBinding(Control.BorderBrushProperty, new Binding(nameof(BorderBrush)) { Source = this });
        popupChrome.SetBinding(Control.BorderThicknessProperty, new Binding(nameof(BorderThickness)) { Source = this });
        popupChrome.SetBinding(Control.PaddingProperty, new Binding(nameof(Padding)) { Source = this });
        popupChrome.SetBinding(Control.FontFamilyProperty, new Binding(nameof(FontFamily)) { Source = this });
        popupChrome.SetBinding(Control.FontSizeProperty, new Binding(nameof(FontSize)) { Source = this });
        popupChrome.SetBinding(Control.FontWeightProperty, new Binding(nameof(FontWeight)) { Source = this });
        popupChrome.SetBinding(NTooltipPopup.ArrowSizeProperty, new Binding(nameof(ArrowSize)) { Source = this });
        popupChrome.SetBinding(NTooltipPopup.CornerRadiusProperty, new Binding(nameof(CornerRadius)) { Source = this });
        popupChrome.SetBinding(NTooltipPopup.RawProperty, new Binding(nameof(Raw)) { Source = this });

        popupChrome.SizeChanged += HandlePopupChromeSizeChanged;
        popupChrome.MouseEnter += HandlePopupChromeMouseEnter;
        popupChrome.MouseLeave += HandlePopupChromeMouseLeave;
        popupChrome.PreviewKeyDown += HandlePopupChromePreviewKeyDown;

        popup = new Popup
        {
            AllowsTransparency = true,
            Child = popupChrome,
            CustomPopupPlacementCallback = HandlePopupPlacement,
            Focusable = false,
            Placement = PlacementMode.Custom,
            PlacementTarget = this,
            PopupAnimation = PopupAnimation.None,
            StaysOpen = true
        };

        popup.Opened += HandlePopupOpened;
        popup.Closed += HandlePopupClosed;

        openTimer = new DispatcherTimer(DispatcherPriority.Background);
        openTimer.Tick += HandleOpenTimerTick;

        closeTimer = new DispatcherTimer(DispatcherPriority.Background);
        closeTimer.Tick += HandleCloseTimerTick;

        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        IsEnabledChanged += HandleIsEnabledChanged;
        IsVisibleChanged += HandleIsVisibleChanged;
        SizeChanged += HandleOwnerSizeChanged;

        UpdateTimerIntervals();
        UpdatePopupBehavior();
        UpdatePopupSizing();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public object? TooltipContent
    {
        get => GetValue(TooltipContentProperty);
        set => SetValue(TooltipContentProperty, value);
    }

    public static readonly DependencyProperty TooltipContentProperty =
        ElementBase.Property<NTooltip, object?>(nameof(TooltipContentProperty), null, OnTooltipConfigChanged);

    public NTooltipTrigger Trigger
    {
        get => (NTooltipTrigger)GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    public static readonly DependencyProperty TriggerProperty =
        ElementBase.Property<NTooltip, NTooltipTrigger>(nameof(TriggerProperty), NTooltipTrigger.Hover, OnTooltipConfigChanged);

    public NTooltipPlacement Placement
    {
        get => (NTooltipPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public static readonly DependencyProperty PlacementProperty =
        ElementBase.Property<NTooltip, NTooltipPlacement>(nameof(PlacementProperty), NTooltipPlacement.Top, OnTooltipConfigChanged);

    public bool Show
    {
        get => (bool)GetValue(ShowProperty);
        set => SetValue(ShowProperty, value);
    }

    public static readonly DependencyProperty ShowProperty =
        DependencyProperty.Register(
            nameof(Show),
            typeof(bool),
            typeof(NTooltip),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnShowChanged));

    public bool Disabled
    {
        get => (bool)GetValue(DisabledProperty);
        set => SetValue(DisabledProperty, value);
    }

    public static readonly DependencyProperty DisabledProperty =
        ElementBase.Property<NTooltip, bool>(nameof(DisabledProperty), false, OnTooltipConfigChanged);

    public bool ShowArrow
    {
        get => (bool)GetValue(ShowArrowProperty);
        set => SetValue(ShowArrowProperty, value);
    }

    public static readonly DependencyProperty ShowArrowProperty =
        ElementBase.Property<NTooltip, bool>(nameof(ShowArrowProperty), true, OnTooltipConfigChanged);

    public bool Raw
    {
        get => (bool)GetValue(RawProperty);
        set => SetValue(RawProperty, value);
    }

    public static readonly DependencyProperty RawProperty =
        ElementBase.Property<NTooltip, bool>(nameof(RawProperty), false, OnTooltipConfigChanged);

    public bool KeepAliveOnHover
    {
        get => (bool)GetValue(KeepAliveOnHoverProperty);
        set => SetValue(KeepAliveOnHoverProperty, value);
    }

    public static readonly DependencyProperty KeepAliveOnHoverProperty =
        ElementBase.Property<NTooltip, bool>(nameof(KeepAliveOnHoverProperty), true, OnTooltipConfigChanged);

    public int OpenDelay
    {
        get => (int)GetValue(OpenDelayProperty);
        set => SetValue(OpenDelayProperty, value);
    }

    public static readonly DependencyProperty OpenDelayProperty =
        DependencyProperty.Register(
            nameof(OpenDelay),
            typeof(int),
            typeof(NTooltip),
            new PropertyMetadata(100, OnTooltipConfigChanged, CoerceNonNegativeInt));

    public int CloseDelay
    {
        get => (int)GetValue(CloseDelayProperty);
        set => SetValue(CloseDelayProperty, value);
    }

    public static readonly DependencyProperty CloseDelayProperty =
        DependencyProperty.Register(
            nameof(CloseDelay),
            typeof(int),
            typeof(NTooltip),
            new PropertyMetadata(100, OnTooltipConfigChanged, CoerceNonNegativeInt));

    public double Distance
    {
        get => (double)GetValue(DistanceProperty);
        set => SetValue(DistanceProperty, value);
    }

    public static readonly DependencyProperty DistanceProperty =
        DependencyProperty.Register(
            nameof(Distance),
            typeof(double),
            typeof(NTooltip),
            new PropertyMetadata(8d, OnTooltipConfigChanged, CoerceNonNegativeDouble));

    public bool Overlap
    {
        get => (bool)GetValue(OverlapProperty);
        set => SetValue(OverlapProperty, value);
    }

    public static readonly DependencyProperty OverlapProperty =
        ElementBase.Property<NTooltip, bool>(nameof(OverlapProperty), false, OnTooltipConfigChanged);

    public bool ArrowPointToCenter
    {
        get => (bool)GetValue(ArrowPointToCenterProperty);
        set => SetValue(ArrowPointToCenterProperty, value);
    }

    public static readonly DependencyProperty ArrowPointToCenterProperty =
        ElementBase.Property<NTooltip, bool>(nameof(ArrowPointToCenterProperty), false, OnTooltipConfigChanged);

    public double HorizontalOffset
    {
        get => (double)GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    public static readonly DependencyProperty HorizontalOffsetProperty =
        ElementBase.Property<NTooltip, double>(nameof(HorizontalOffsetProperty), 0d, OnTooltipConfigChanged);

    public double VerticalOffset
    {
        get => (double)GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    public static readonly DependencyProperty VerticalOffsetProperty =
        ElementBase.Property<NTooltip, double>(nameof(VerticalOffsetProperty), 0d, OnTooltipConfigChanged);

    public double PopupMinWidth
    {
        get => (double)GetValue(PopupMinWidthProperty);
        set => SetValue(PopupMinWidthProperty, value);
    }

    public static readonly DependencyProperty PopupMinWidthProperty =
        DependencyProperty.Register(
            nameof(PopupMinWidth),
            typeof(double),
            typeof(NTooltip),
            new PropertyMetadata(0d, OnTooltipConfigChanged, CoerceNonNegativeDouble));

    public double PopupMaxWidth
    {
        get => (double)GetValue(PopupMaxWidthProperty);
        set => SetValue(PopupMaxWidthProperty, value);
    }

    public static readonly DependencyProperty PopupMaxWidthProperty =
        DependencyProperty.Register(
            nameof(PopupMaxWidth),
            typeof(double),
            typeof(NTooltip),
            new PropertyMetadata(320d, OnTooltipConfigChanged, CoerceNonNegativeDouble));

    public bool StretchToTriggerWidth
    {
        get => (bool)GetValue(StretchToTriggerWidthProperty);
        set => SetValue(StretchToTriggerWidthProperty, value);
    }

    public static readonly DependencyProperty StretchToTriggerWidthProperty =
        ElementBase.Property<NTooltip, bool>(nameof(StretchToTriggerWidthProperty), false, OnTooltipConfigChanged);

    public double ArrowSize
    {
        get => (double)GetValue(ArrowSizeProperty);
        set => SetValue(ArrowSizeProperty, value);
    }

    public static readonly DependencyProperty ArrowSizeProperty =
        DependencyProperty.Register(
            nameof(ArrowSize),
            typeof(double),
            typeof(NTooltip),
            new PropertyMetadata(8d, OnTooltipConfigChanged, CoerceNonNegativeDouble));

    public double ArrowEdgeMargin
    {
        get => (double)GetValue(ArrowEdgeMarginProperty);
        set => SetValue(ArrowEdgeMarginProperty, value);
    }

    public static readonly DependencyProperty ArrowEdgeMarginProperty =
        DependencyProperty.Register(
            nameof(ArrowEdgeMargin),
            typeof(double),
            typeof(NTooltip),
            new PropertyMetadata(12d, OnTooltipConfigChanged, CoerceNonNegativeDouble));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        ElementBase.Property<NTooltip, CornerRadius>(nameof(CornerRadiusProperty), new CornerRadius(6), OnTooltipConfigChanged);

    public event EventHandler? Opened;

    public event EventHandler? Closed;

    public void Open()
    {
        SetShowState(true);
    }

    public void Close()
    {
        SetShowState(false);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdatePopupBehavior();
        UpdatePopupSizing();
        UpdatePopupChromeLayout();
        SyncShowToPopup();
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        CancelCloseTimer();

        if (Trigger == NTooltipTrigger.Hover)
        {
            StartOpenTimer();
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        CancelOpenTimer();

        if (Trigger == NTooltipTrigger.Hover)
        {
            StartCloseTimer();
        }
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);

        if (Trigger != NTooltipTrigger.Click || Disabled || !IsEnabled)
        {
            return;
        }

        if (Show)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnIsKeyboardFocusWithinChanged(e);

        if (Trigger != NTooltipTrigger.Focus || Disabled || !IsEnabled)
        {
            return;
        }

        CancelOpenTimer();

        if ((bool)e.NewValue)
        {
            Open();
        }
        else
        {
            StartCloseTimer();
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Key == Key.Escape && Show)
        {
            Close();
            e.Handled = true;
        }
    }

    private static void OnShowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NTooltip tooltip && !tooltip.synchronizingShow)
        {
            tooltip.SyncShowToPopup();
        }
    }

    private static void OnTooltipConfigChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NTooltip tooltip)
        {
            return;
        }

        tooltip.UpdateTimerIntervals();
        tooltip.UpdatePopupBehavior();
        tooltip.UpdatePopupSizing();
        tooltip.UpdatePopupChromeLayout();
        tooltip.SyncShowToPopup();

        if (tooltip.popup.IsOpen)
        {
            tooltip.RequestPopupReposition();
        }
    }

    private static object CoerceNonNegativeDouble(DependencyObject d, object baseValue)
    {
        if (baseValue is double value)
        {
            if (double.IsPositiveInfinity(value))
            {
                return value;
            }

            if (double.IsNaN(value) || value < 0d)
            {
                return 0d;
            }
        }

        return baseValue;
    }

    private static object CoerceNonNegativeInt(DependencyObject d, object baseValue)
    {
        return baseValue is int value && value < 0 ? 0 : baseValue;
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        UpdateClickDismissBehavior();
        SyncShowToPopup();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        CancelOpenTimer();
        CancelCloseTimer();
        DetachClickDismissHandlers();
        popup.IsOpen = false;
    }

    private void HandleIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        SyncShowToPopup();
    }

    private void HandleIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
        {
            SyncShowToPopup();
            return;
        }

        if (popup.IsOpen)
        {
            popup.IsOpen = false;
        }
    }

    private void HandleOwnerSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdatePopupSizing();
        UpdatePopupChromeLayout();

        if (popup.IsOpen)
        {
            RequestPopupReposition();
        }
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        CancelCloseTimer();
        CancelOpenTimer();
        UpdatePopupChromeLayout();
        UpdateClickDismissBehavior();
        Opened?.Invoke(this, EventArgs.Empty);
    }

    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        CancelCloseTimer();
        CancelOpenTimer();
        UpdateClickDismissBehavior();

        if (Show)
        {
            synchronizingShow = true;
            SetCurrentValue(ShowProperty, false);
            synchronizingShow = false;
        }

        Closed?.Invoke(this, EventArgs.Empty);
    }

    private void HandlePopupChromeSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdatePopupChromeLayout();
    }

    private void HandlePopupChromeMouseEnter(object sender, MouseEventArgs e)
    {
        if (Trigger == NTooltipTrigger.Hover && KeepAliveOnHover)
        {
            CancelCloseTimer();
        }
    }

    private void HandlePopupChromeMouseLeave(object sender, MouseEventArgs e)
    {
        if (Trigger == NTooltipTrigger.Hover && KeepAliveOnHover)
        {
            StartCloseTimer();
        }
    }

    private void HandlePopupChromePreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && Show)
        {
            Close();
            e.Handled = true;
        }
    }

    private void HandleOpenTimerTick(object? sender, EventArgs e)
    {
        openTimer.Stop();

        if (Trigger != NTooltipTrigger.Hover || Disabled || !IsEnabled)
        {
            return;
        }

        if (IsMouseOver)
        {
            Open();
        }
    }

    private void HandleCloseTimerTick(object? sender, EventArgs e)
    {
        closeTimer.Stop();

        switch (Trigger)
        {
            case NTooltipTrigger.Hover:
                if (IsMouseOver || KeepAliveOnHover && IsPointerWithinPopup())
                {
                    return;
                }

                break;
            case NTooltipTrigger.Focus:
                if (IsKeyboardFocusWithin)
                {
                    return;
                }

                break;
        }

        Close();
    }

    private void StartOpenTimer()
    {
        if (Trigger != NTooltipTrigger.Hover || Show || Disabled || !IsEnabled || !HasTooltipContent())
        {
            return;
        }

        openTimer.Stop();
        if (OpenDelay <= 0)
        {
            Open();
            return;
        }

        openTimer.Start();
    }

    private void StartCloseTimer()
    {
        if (!Show)
        {
            return;
        }

        closeTimer.Stop();
        if (CloseDelay <= 0)
        {
            HandleCloseTimerTick(this, EventArgs.Empty);
            return;
        }

        closeTimer.Start();
    }

    private void CancelOpenTimer()
    {
        openTimer.Stop();
    }

    private void CancelCloseTimer()
    {
        closeTimer.Stop();
    }

    private void SetShowState(bool value)
    {
        if (Show == value && popup.IsOpen == value)
        {
            return;
        }

        synchronizingShow = true;
        SetCurrentValue(ShowProperty, value);
        synchronizingShow = false;
        SyncShowToPopup();
    }

    private void SyncShowToPopup()
    {
        UpdatePopupBehavior();
        UpdatePopupSizing();

        if (Show && CanOpen())
        {
            if (!popup.IsOpen)
            {
                popup.IsOpen = true;
            }

            UpdatePopupChromeLayout();
            UpdateClickDismissBehavior();
            return;
        }

        if (popup.IsOpen)
        {
            popup.IsOpen = false;
        }

        UpdateClickDismissBehavior();

        if (Show && (!HasTooltipContent() || Disabled || !IsEnabled))
        {
            synchronizingShow = true;
            SetCurrentValue(ShowProperty, false);
            synchronizingShow = false;
        }
    }

    private bool CanOpen()
    {
        return IsLoaded && IsVisible && IsEnabled && !Disabled && HasTooltipContent();
    }

    private bool HasTooltipContent()
    {
        return TooltipContent switch
        {
            null => false,
            string text => !string.IsNullOrWhiteSpace(text),
            _ => true
        };
    }

    private void UpdateTimerIntervals()
    {
        openTimer.Interval = TimeSpan.FromMilliseconds(OpenDelay);
        closeTimer.Interval = TimeSpan.FromMilliseconds(CloseDelay);
    }

    private void UpdatePopupBehavior()
    {
        popup.StaysOpen = true;
        UpdateClickDismissBehavior();
    }

    private void UpdatePopupSizing()
    {
        var minWidth = Math.Max(0d, PopupMinWidth);
        var maxWidth = double.IsPositiveInfinity(PopupMaxWidth)
            ? double.PositiveInfinity
            : Math.Max(minWidth, PopupMaxWidth);

        popupChrome.MinWidth = minWidth;
        popupChrome.MaxWidth = maxWidth;
        popupChrome.Width = StretchToTriggerWidth && ActualWidth > 0d ? ActualWidth : double.NaN;
    }

    private CustomPopupPlacement[] HandlePopupPlacement(Size popupSize, Size targetSize, Point offset)
    {
        const double LeftPlacementVisualGap = 2d;
        var horizontalOffset = HorizontalOffset;
        var verticalOffset = VerticalOffset;
        var distance = Overlap ? 0d : Distance;
        var arrowGap = ShowArrow && !Raw ? Math.Max(0d, ArrowSize) : 0d;

        return Placement switch
        {
            NTooltipPlacement.Top => [new CustomPopupPlacement(new Point((targetSize.Width - popupSize.Width) / 2d + horizontalOffset, -popupSize.Height + arrowGap - distance + verticalOffset), PopupPrimaryAxis.Vertical)],
            NTooltipPlacement.TopStart => [new CustomPopupPlacement(new Point(horizontalOffset, -popupSize.Height + arrowGap - distance + verticalOffset), PopupPrimaryAxis.Vertical)],
            NTooltipPlacement.TopEnd => [new CustomPopupPlacement(new Point(targetSize.Width - popupSize.Width + horizontalOffset, -popupSize.Height + arrowGap - distance + verticalOffset), PopupPrimaryAxis.Vertical)],
            NTooltipPlacement.Right => [new CustomPopupPlacement(new Point(targetSize.Width - arrowGap + distance + horizontalOffset, (targetSize.Height - popupSize.Height) / 2d + verticalOffset), PopupPrimaryAxis.Horizontal)],
            NTooltipPlacement.RightStart => [new CustomPopupPlacement(new Point(targetSize.Width - arrowGap + distance + horizontalOffset, verticalOffset), PopupPrimaryAxis.Horizontal)],
            NTooltipPlacement.RightEnd => [new CustomPopupPlacement(new Point(targetSize.Width - arrowGap + distance + horizontalOffset, targetSize.Height - popupSize.Height + verticalOffset), PopupPrimaryAxis.Horizontal)],
            NTooltipPlacement.BottomStart => [new CustomPopupPlacement(new Point(horizontalOffset, targetSize.Height - arrowGap + distance + verticalOffset), PopupPrimaryAxis.Vertical)],
            NTooltipPlacement.BottomEnd => [new CustomPopupPlacement(new Point(targetSize.Width - popupSize.Width + horizontalOffset, targetSize.Height - arrowGap + distance + verticalOffset), PopupPrimaryAxis.Vertical)],
            NTooltipPlacement.Left => [new CustomPopupPlacement(new Point(-popupSize.Width + arrowGap - distance - LeftPlacementVisualGap + horizontalOffset, (targetSize.Height - popupSize.Height) / 2d + verticalOffset), PopupPrimaryAxis.Horizontal)],
            NTooltipPlacement.LeftStart => [new CustomPopupPlacement(new Point(-popupSize.Width + arrowGap - distance - LeftPlacementVisualGap + horizontalOffset, verticalOffset), PopupPrimaryAxis.Horizontal)],
            NTooltipPlacement.LeftEnd => [new CustomPopupPlacement(new Point(-popupSize.Width + arrowGap - distance - LeftPlacementVisualGap + horizontalOffset, targetSize.Height - popupSize.Height + verticalOffset), PopupPrimaryAxis.Horizontal)],
            _ => [new CustomPopupPlacement(new Point((targetSize.Width - popupSize.Width) / 2d + horizontalOffset, targetSize.Height - arrowGap + distance + verticalOffset), PopupPrimaryAxis.Vertical)]
        };
    }

    private void UpdatePopupChromeLayout()
    {
        const double ArrowOverlap = 1d;
        var showArrow = ShowArrow && !Raw;
        if (!showArrow)
        {
            popupChrome.UpdateChromeLayout(false, new Thickness(0), 0d, 0d, 0d, 0d, Geometry.Empty);
            return;
        }

        var arrowSize = Math.Max(0d, ArrowSize);
        var verticalArrowWidth = arrowSize * 2d;
        var verticalArrowHeight = arrowSize;
        var horizontalArrowWidth = arrowSize;
        var horizontalArrowHeight = arrowSize * 2d;
        var popupMargin = Placement switch
        {
            NTooltipPlacement.Top or NTooltipPlacement.TopStart or NTooltipPlacement.TopEnd => new Thickness(0, 0, 0, arrowSize),
            NTooltipPlacement.Right or NTooltipPlacement.RightStart or NTooltipPlacement.RightEnd => new Thickness(arrowSize, 0, 0, 0),
            NTooltipPlacement.Left or NTooltipPlacement.LeftStart or NTooltipPlacement.LeftEnd => new Thickness(0, 0, arrowSize, 0),
            _ => new Thickness(0, arrowSize, 0, 0)
        };

        var popupWidth = Math.Max(0d, popupChrome.ActualWidth - popupMargin.Left - popupMargin.Right);
        var popupHeight = Math.Max(0d, popupChrome.ActualHeight - popupMargin.Top - popupMargin.Bottom);
        if (popupWidth <= 0d || popupHeight <= 0d)
        {
            popupChrome.UpdateChromeLayout(true, popupMargin, 0d, 0d, 0d, 0d, Geometry.Empty);
            return;
        }

        var isVerticalPlacement = Placement is NTooltipPlacement.Top or NTooltipPlacement.TopStart or NTooltipPlacement.TopEnd
            or NTooltipPlacement.Bottom or NTooltipPlacement.BottomStart or NTooltipPlacement.BottomEnd;
        var arrowWidth = isVerticalPlacement ? verticalArrowWidth : horizontalArrowWidth;
        var arrowHeight = isVerticalPlacement ? verticalArrowHeight : horizontalArrowHeight;
        var maxHorizontal = Math.Max(0d, popupWidth - arrowWidth);
        var maxVertical = Math.Max(0d, popupHeight - arrowHeight);
        var edgeMargin = Math.Max(0d, ArrowEdgeMargin);

        double left;
        double top;
        Geometry arrowData;

        switch (Placement)
        {
            case NTooltipPlacement.Top:
            case NTooltipPlacement.Bottom:
                left = popupMargin.Left + Clamp((popupWidth - arrowWidth) / 2d, 0d, maxHorizontal);
                top = Placement == NTooltipPlacement.Top
                    ? popupMargin.Top + popupHeight - ArrowOverlap
                    : popupMargin.Top - arrowHeight + ArrowOverlap;
                arrowData = Placement == NTooltipPlacement.Top ? ArrowDownGeometry : ArrowUpGeometry;
                break;
            case NTooltipPlacement.TopStart:
            case NTooltipPlacement.BottomStart:
                left = popupMargin.Left + (ArrowPointToCenter
                    ? Clamp(ActualWidth / 2d - arrowWidth / 2d, 0d, maxHorizontal)
                    : Clamp(edgeMargin, 0d, maxHorizontal));
                top = Placement == NTooltipPlacement.TopStart
                    ? popupMargin.Top + popupHeight - ArrowOverlap
                    : popupMargin.Top - arrowHeight + ArrowOverlap;
                arrowData = Placement == NTooltipPlacement.TopStart ? ArrowDownGeometry : ArrowUpGeometry;
                break;
            case NTooltipPlacement.TopEnd:
            case NTooltipPlacement.BottomEnd:
                left = popupMargin.Left + (ArrowPointToCenter
                    ? Clamp(popupWidth - ActualWidth / 2d - arrowWidth / 2d, 0d, maxHorizontal)
                    : Clamp(maxHorizontal - edgeMargin, 0d, maxHorizontal));
                top = Placement == NTooltipPlacement.TopEnd
                    ? popupMargin.Top + popupHeight - ArrowOverlap
                    : popupMargin.Top - arrowHeight + ArrowOverlap;
                arrowData = Placement == NTooltipPlacement.TopEnd ? ArrowDownGeometry : ArrowUpGeometry;
                break;
            case NTooltipPlacement.Left:
            case NTooltipPlacement.Right:
                left = Placement == NTooltipPlacement.Left
                    ? popupMargin.Left + popupWidth - ArrowOverlap
                    : popupMargin.Left - arrowWidth + ArrowOverlap;
                top = popupMargin.Top + Clamp((popupHeight - arrowHeight) / 2d, 0d, maxVertical);
                arrowData = Placement == NTooltipPlacement.Left ? ArrowRightGeometry : ArrowLeftGeometry;
                break;
            case NTooltipPlacement.LeftStart:
            case NTooltipPlacement.RightStart:
                left = Placement == NTooltipPlacement.LeftStart
                    ? popupMargin.Left + popupWidth - ArrowOverlap
                    : popupMargin.Left - arrowWidth + ArrowOverlap;
                top = popupMargin.Top + (ArrowPointToCenter
                    ? Clamp(ActualHeight / 2d - arrowHeight / 2d, 0d, maxVertical)
                    : Clamp(edgeMargin, 0d, maxVertical));
                arrowData = Placement == NTooltipPlacement.LeftStart ? ArrowRightGeometry : ArrowLeftGeometry;
                break;
            case NTooltipPlacement.LeftEnd:
            case NTooltipPlacement.RightEnd:
                left = Placement == NTooltipPlacement.LeftEnd
                    ? popupMargin.Left + popupWidth - ArrowOverlap
                    : popupMargin.Left - arrowWidth + ArrowOverlap;
                top = popupMargin.Top + (ArrowPointToCenter
                    ? Clamp(popupHeight - ActualHeight / 2d - arrowHeight / 2d, 0d, maxVertical)
                    : Clamp(maxVertical - edgeMargin, 0d, maxVertical));
                arrowData = Placement == NTooltipPlacement.LeftEnd ? ArrowRightGeometry : ArrowLeftGeometry;
                break;
            default:
                left = popupMargin.Left + Clamp((popupWidth - arrowWidth) / 2d, 0d, maxHorizontal);
                top = -ArrowOverlap;
                arrowData = ArrowUpGeometry;
                break;
        }

        popupChrome.UpdateChromeLayout(true, popupMargin, left, top, arrowWidth, arrowHeight, arrowData);
    }

    private void RequestPopupReposition()
    {
        Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            new Action(() =>
            {
                if (!popup.IsOpen)
                {
                    return;
                }

                UpdatePopupChromeLayout();
                popup.HorizontalOffset += 0.01d;
                popup.HorizontalOffset -= 0.01d;
            }));
    }

    private bool IsPointerWithinPopup()
    {
        return IsSourceWithinElement(Mouse.DirectlyOver as DependencyObject, popupChrome);
    }

    private void UpdateClickDismissBehavior()
    {
        if (Trigger == NTooltipTrigger.Click && popup.IsOpen)
        {
            AttachClickDismissHandlers();
            return;
        }

        DetachClickDismissHandlers();
    }

    private void AttachClickDismissHandlers()
    {
        var currentWindow = Window.GetWindow(this);
        if (ReferenceEquals(currentWindow, ownerWindow))
        {
            return;
        }

        if (ownerWindow is not null)
        {
            ownerWindow.PreviewMouseLeftButtonDown -= HandleOwnerWindowPreviewMouseLeftButtonDown;
            ownerWindow.PreviewMouseRightButtonDown -= HandleOwnerWindowPreviewMouseRightButtonDown;
            ownerWindow.Deactivated -= HandleOwnerWindowDeactivated;
        }

        ownerWindow = currentWindow;
        if (ownerWindow is not null)
        {
            ownerWindow.PreviewMouseLeftButtonDown += HandleOwnerWindowPreviewMouseLeftButtonDown;
            ownerWindow.PreviewMouseRightButtonDown += HandleOwnerWindowPreviewMouseRightButtonDown;
            ownerWindow.Deactivated += HandleOwnerWindowDeactivated;
        }
    }

    private void DetachClickDismissHandlers()
    {
        if (ownerWindow is not null)
        {
            ownerWindow.PreviewMouseLeftButtonDown -= HandleOwnerWindowPreviewMouseLeftButtonDown;
            ownerWindow.PreviewMouseRightButtonDown -= HandleOwnerWindowPreviewMouseRightButtonDown;
            ownerWindow.Deactivated -= HandleOwnerWindowDeactivated;
            ownerWindow = null;
        }
    }

    private void HandleOwnerWindowPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        HandleOwnerWindowPreviewMouseButtonDown(e);
    }

    private void HandleOwnerWindowPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        HandleOwnerWindowPreviewMouseButtonDown(e);
    }

    private void HandleOwnerWindowPreviewMouseButtonDown(MouseButtonEventArgs e)
    {
        if (Trigger != NTooltipTrigger.Click || !popup.IsOpen || !Show)
        {
            return;
        }

        if (IsSourceWithinElement(e.OriginalSource as DependencyObject, this))
        {
            return;
        }

        Close();
    }

    private void HandleOwnerWindowDeactivated(object? sender, EventArgs e)
    {
        if (Trigger == NTooltipTrigger.Click && Show)
        {
            Close();
        }
    }
    private static bool IsSourceWithinElement(DependencyObject? source, DependencyObject target)
    {
        var current = source;
        while (current is not null)
        {
            if (ReferenceEquals(current, target))
            {
                return true;
            }

            current = LogicalTreeHelper.GetParent(current)
                      ?? (current is Visual or Visual3D
                          ? VisualTreeHelper.GetParent(current)
                          : null);
        }

        return false;
    }

    private static double Clamp(double value, double min, double max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

    private static Geometry CreateFrozenGeometry(string data)
    {
        var geometry = Geometry.Parse(data);
        geometry.Freeze();
        return geometry;
    }
}
