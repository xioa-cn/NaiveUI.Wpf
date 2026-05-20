using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NDropdown : ContentControl
{
    private const double ArrowSize = 12d;
    private const double ArrowOffset = 6d;
    private readonly DispatcherTimer hoverCloseTimer;
    private readonly NDropdownMenu menu;
    private bool synchronizingShow;
    private Point contextMenuPoint;

    static NDropdown()
    {
        ElementBase.DefaultStyle<NDropdown>(DefaultStyleKeyProperty);
    }

    public NDropdown()
    {
        menu = new NDropdownMenu
        {
            OwnerDropdown = this,
            Placement = PlacementMode.Custom,
            PlacementTarget = this,
            CustomPopupPlacementCallback = HandlePopupPlacement,
            StaysOpen = false
        };

        menu.Opened += HandleMenuOpened;
        menu.Closed += HandleMenuClosed;
        menu.SizeChanged += HandleMenuSizeChanged;

        Options.CollectionChanged += HandleOptionsCollectionChanged;
        DataContextChanged += HandleDropdownDataContextChanged;

        hoverCloseTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(140)
        };
        hoverCloseTimer.Tick += HandleHoverCloseTimerTick;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<NDropdownOptionBase> Options { get; } = [];

    public NDropdownTrigger Trigger
    {
        get => (NDropdownTrigger)GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    public static readonly DependencyProperty TriggerProperty =
        ElementBase.Property<NDropdown, NDropdownTrigger>(nameof(TriggerProperty), NDropdownTrigger.Hover, OnPopupConfigChanged);

    public NDropdownPlacement Placement
    {
        get => (NDropdownPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public static readonly DependencyProperty PlacementProperty =
        ElementBase.Property<NDropdown, NDropdownPlacement>(nameof(PlacementProperty), NDropdownPlacement.Bottom, OnPopupConfigChanged);

    public NDropdownSize Size
    {
        get => (NDropdownSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        ElementBase.Property<NDropdown, NDropdownSize>(nameof(SizeProperty), NDropdownSize.Medium, OnPopupConfigChanged);

    public bool ShowArrow
    {
        get => (bool)GetValue(ShowArrowProperty);
        set => SetValue(ShowArrowProperty, value);
    }

    public static readonly DependencyProperty ShowArrowProperty =
        ElementBase.Property<NDropdown, bool>(nameof(ShowArrowProperty), false, OnPopupConfigChanged);

    public bool Show
    {
        get => (bool)GetValue(ShowProperty);
        set => SetValue(ShowProperty, value);
    }

    public static readonly DependencyProperty ShowProperty =
        DependencyProperty.Register(
            nameof(Show),
            typeof(bool),
            typeof(NDropdown),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnShowChanged));

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(object),
            typeof(NDropdown),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public double MaxMenuHeight
    {
        get => (double)GetValue(MaxMenuHeightProperty);
        set => SetValue(MaxMenuHeightProperty, value);
    }

    public static readonly DependencyProperty MaxMenuHeightProperty =
        ElementBase.Property<NDropdown, double>(nameof(MaxMenuHeightProperty), 320d, OnPopupConfigChanged);

    public double HorizontalOffset
    {
        get => (double)GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    public static readonly DependencyProperty HorizontalOffsetProperty =
        ElementBase.Property<NDropdown, double>(nameof(HorizontalOffsetProperty), 0d, OnPopupConfigChanged);

    public double VerticalOffset
    {
        get => (double)GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    public static readonly DependencyProperty VerticalOffsetProperty =
        ElementBase.Property<NDropdown, double>(nameof(VerticalOffsetProperty), 0d, OnPopupConfigChanged);

    public event EventHandler<NDropdownSelectEventArgs>? Select;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        RefreshEntries();
        SyncShowToMenu();
        UpdateMenuChromeLayout();
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        CancelHoverCloseTimer();

        if (Trigger == NDropdownTrigger.Hover)
        {
            Open();
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        if (Trigger == NDropdownTrigger.Hover)
        {
            StartHoverCloseTimer();
        }
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);

        if (Trigger != NDropdownTrigger.Click || !IsEnabled)
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

        e.Handled = true;
    }

    protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseRightButtonDown(e);

        if (Trigger != NDropdownTrigger.ContextMenu || !IsEnabled)
        {
            return;
        }

        contextMenuPoint = e.GetPosition(this);
        Open();
        e.Handled = true;
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

    internal void CancelHoverCloseTimer()
    {
        hoverCloseTimer.Stop();
    }

    internal void RefreshSelectionStates()
    {
        menu.RefreshSelectionStatesRecursive();
    }

    internal bool IsSelectedKey(object key)
    {
        return Value is not null && Equals(Value, key);
    }

    internal bool ContainsSelectedDescendant(DropdownEntry entry)
    {
        if (Value is null)
        {
            return false;
        }

        foreach (var child in entry.Children)
        {
            if (child.ContainsKey(Value))
            {
                return true;
            }
        }

        return false;
    }

    internal bool InvokeEntryAction(DropdownEntry entry)
    {
        if (entry.SourceOption is null)
        {
            return false;
        }

        return entry.SourceOption.TryInvoke(this);
    }

    internal void HandleEntryInvoked(DropdownEntry entry, bool updateSelection = true, bool closeAfterInvoke = true)
    {
        if (!InvokeEntryAction(entry))
        {
            return;
        }

        if (updateSelection && entry.SourceOption is not null)
        {
            SetCurrentValue(ValueProperty, entry.Key);
            Select?.Invoke(this, new NDropdownSelectEventArgs(entry.Key, entry.SourceOption));
        }

        if (closeAfterInvoke)
        {
            Close();
            return;
        }

        RefreshSelectionStates();
    }

    private static void OnShowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NDropdown dropdown && !dropdown.synchronizingShow)
        {
            dropdown.SyncShowToMenu();
        }
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NDropdown dropdown)
        {
            dropdown.RefreshSelectionStates();
        }
    }

    private static void OnPopupConfigChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NDropdown dropdown)
        {
            return;
        }

        dropdown.menu.OwnerDropdown = dropdown;

        if (e.Property == SizeProperty)
        {
            dropdown.RefreshEntries();
        }

        if (dropdown.Show)
        {
            dropdown.SyncShowToMenu();
            dropdown.UpdateMenuChromeLayout();
        }
    }

    private void HandleOptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshEntries();

        if (Show)
        {
            SyncShowToMenu();
        }
    }

    private void HandleDropdownDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        menu.OwnerDropdown = this;
        RefreshEntries();

        if (Show)
        {
            UpdateMenuChromeLayout();
        }
    }

    private void Open()
    {
        if (!CanOpen())
        {
            return;
        }

        SetShowState(true);
    }

    private void Close()
    {
        SetShowState(false);
    }

    private void SetShowState(bool value)
    {
        synchronizingShow = true;
        SetCurrentValue(ShowProperty, value);
        synchronizingShow = false;
        SyncShowToMenu();
    }

    private bool CanOpen()
    {
        return IsEnabled && BuildEntries(Options).Count > 0;
    }

    private void SyncShowToMenu()
    {
        if (Show && CanOpen())
        {
            RefreshEntries();
            menu.OwnerDropdown = this;
            menu.IsOpen = true;
            UpdateMenuChromeLayout();
            return;
        }

        if (menu.IsOpen)
        {
            menu.IsOpen = false;
        }

        if (Show && !CanOpen())
        {
            synchronizingShow = true;
            SetCurrentValue(ShowProperty, false);
            synchronizingShow = false;
        }
    }

    private void RefreshEntries()
    {
        menu.OwnerDropdown = this;
        menu.SetEntries(BuildEntries(Options));
        menu.RefreshSelectionStatesRecursive();
    }

    private IReadOnlyList<DropdownEntry> BuildEntries(IEnumerable<NDropdownOptionBase> options)
    {
        var results = new List<DropdownEntry>();

        foreach (var option in options)
        {
            if (!option.Show)
            {
                continue;
            }

            switch (option)
            {
                case NDropdownDividerOption dividerOption:
                    results.Add(new DropdownEntry(NDropdownEntryKind.Divider, dividerOption.Key));
                    break;
                case NDropdownGroupOption groupOption:
                    var groupEntries = BuildEntries(groupOption.Options);
                    if (groupEntries.Count == 0)
                    {
                        break;
                    }

                    var resolvedGroupLabel = groupOption.ResolveLabel(this);
                    var resolvedGroupIcon = groupOption.ResolveIcon(this);

                    if (resolvedGroupLabel is not null || resolvedGroupIcon is not null)
                    {
                        results.Add(new DropdownEntry(NDropdownEntryKind.GroupHeader, groupOption.Key, resolvedGroupLabel, resolvedGroupIcon));
                    }

                    foreach (var child in groupEntries)
                    {
                        results.Add(child);
                    }

                    break;
                case NDropdownRenderOption renderOption:
                    results.Add(new DropdownEntry(NDropdownEntryKind.Render, renderOption.Key, content: renderOption.Content));
                    break;
                case NDropdownCombineOption combineOption:
                    results.Add(
                        new DropdownEntry(
                            NDropdownEntryKind.Option,
                            combineOption.Key,
                            combineOption.ResolveLabel(this),
                            combineOption.ResolveIcon(this),
                            combineOption.ResolveSuffix(this),
                            disabled: combineOption.Disabled,
                            children: BuildEntries(combineOption.Options)));
                    break;
                case NDropdownOption dropdownOption:
                    results.Add(
                        new DropdownEntry(
                            NDropdownEntryKind.Option,
                            dropdownOption.Key,
                            dropdownOption.ResolveLabel(this),
                            dropdownOption.ResolveIcon(this),
                            dropdownOption.ResolveSuffix(this),
                            disabled: dropdownOption.Disabled,
                            sourceOption: dropdownOption));
                    break;
            }
        }

        return results;
    }

    private void HandleMenuOpened(object? sender, RoutedEventArgs e)
    {
        CancelHoverCloseTimer();
        UpdateMenuChromeLayout();
        menu.RefreshSelectionStatesRecursive();
    }

    private void HandleMenuClosed(object? sender, RoutedEventArgs e)
    {
        CancelHoverCloseTimer();

        if (Show)
        {
            synchronizingShow = true;
            SetCurrentValue(ShowProperty, false);
            synchronizingShow = false;
        }
    }

    private void HandleMenuSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateMenuChromeLayout();
    }

    private void StartHoverCloseTimer()
    {
        if (Trigger != NDropdownTrigger.Hover || !Show)
        {
            return;
        }

        hoverCloseTimer.Stop();
        hoverCloseTimer.Start();
    }

    private void HandleHoverCloseTimerTick(object? sender, EventArgs e)
    {
        hoverCloseTimer.Stop();

        if (IsMouseOver || IsPointerWithinMenu())
        {
            return;
        }

        Close();
    }

    private CustomPopupPlacement[] HandlePopupPlacement(Size popupSize, Size targetSize, Point offset)
    {
        var horizontalOffset = HorizontalOffset;
        var verticalOffset = VerticalOffset;

        if (Trigger == NDropdownTrigger.ContextMenu)
        {
            return
            [
                new CustomPopupPlacement(
                    new Point(contextMenuPoint.X + horizontalOffset, contextMenuPoint.Y + verticalOffset),
                    PopupPrimaryAxis.None)
            ];
        }

        return Placement switch
        {
            NDropdownPlacement.Top => [new CustomPopupPlacement(new Point((targetSize.Width - popupSize.Width) / 2d + horizontalOffset, -popupSize.Height + verticalOffset), PopupPrimaryAxis.Vertical)],
            NDropdownPlacement.TopStart => [new CustomPopupPlacement(new Point(horizontalOffset, -popupSize.Height + verticalOffset), PopupPrimaryAxis.Vertical)],
            NDropdownPlacement.TopEnd => [new CustomPopupPlacement(new Point(targetSize.Width - popupSize.Width + horizontalOffset, -popupSize.Height + verticalOffset), PopupPrimaryAxis.Vertical)],
            NDropdownPlacement.Right => [new CustomPopupPlacement(new Point(targetSize.Width + horizontalOffset, (targetSize.Height - popupSize.Height) / 2d + verticalOffset), PopupPrimaryAxis.Horizontal)],
            NDropdownPlacement.RightStart => [new CustomPopupPlacement(new Point(targetSize.Width + horizontalOffset, verticalOffset), PopupPrimaryAxis.Horizontal)],
            NDropdownPlacement.RightEnd => [new CustomPopupPlacement(new Point(targetSize.Width + horizontalOffset, targetSize.Height - popupSize.Height + verticalOffset), PopupPrimaryAxis.Horizontal)],
            NDropdownPlacement.BottomStart => [new CustomPopupPlacement(new Point(horizontalOffset, targetSize.Height + verticalOffset), PopupPrimaryAxis.Vertical)],
            NDropdownPlacement.BottomEnd => [new CustomPopupPlacement(new Point(targetSize.Width - popupSize.Width + horizontalOffset, targetSize.Height + verticalOffset), PopupPrimaryAxis.Vertical)],
            NDropdownPlacement.Left => [new CustomPopupPlacement(new Point(-popupSize.Width + horizontalOffset, (targetSize.Height - popupSize.Height) / 2d + verticalOffset), PopupPrimaryAxis.Horizontal)],
            NDropdownPlacement.LeftStart => [new CustomPopupPlacement(new Point(-popupSize.Width + horizontalOffset, verticalOffset), PopupPrimaryAxis.Horizontal)],
            NDropdownPlacement.LeftEnd => [new CustomPopupPlacement(new Point(-popupSize.Width + horizontalOffset, targetSize.Height - popupSize.Height + verticalOffset), PopupPrimaryAxis.Horizontal)],
            _ => [new CustomPopupPlacement(new Point((targetSize.Width - popupSize.Width) / 2d + horizontalOffset, targetSize.Height + verticalOffset), PopupPrimaryAxis.Vertical)]
        };
    }

    private void UpdateMenuChromeLayout()
    {
        var showArrow = ShowArrow && Trigger != NDropdownTrigger.ContextMenu;
        if (!showArrow)
        {
            menu.UpdateChromeLayout(false, new Thickness(0), 0d, 0d);
            return;
        }

        var menuMargin = Placement switch
        {
            NDropdownPlacement.Top or NDropdownPlacement.TopStart or NDropdownPlacement.TopEnd => new Thickness(0, 0, 0, ArrowOffset),
            NDropdownPlacement.Right or NDropdownPlacement.RightStart or NDropdownPlacement.RightEnd => new Thickness(ArrowOffset, 0, 0, 0),
            NDropdownPlacement.Left or NDropdownPlacement.LeftStart or NDropdownPlacement.LeftEnd => new Thickness(0, 0, ArrowOffset, 0),
            _ => new Thickness(0, ArrowOffset, 0, 0)
        };

        var menuWidth = Math.Max(0d, menu.ActualWidth - menuMargin.Left - menuMargin.Right);
        var menuHeight = Math.Max(0d, menu.ActualHeight - menuMargin.Top - menuMargin.Bottom);
        if (menuWidth <= 0d || menuHeight <= 0d)
        {
            menu.UpdateChromeLayout(true, menuMargin, 0d, 0d);
            return;
        }

        var triggerWidth = ActualWidth;
        var triggerHeight = ActualHeight;

        double left;
        double top;

        switch (Placement)
        {
            case NDropdownPlacement.Top:
            case NDropdownPlacement.Bottom:
                left = menuMargin.Left + (menuWidth - ArrowSize) / 2d;
                top = Placement == NDropdownPlacement.Top ? menuMargin.Top + menuHeight - ArrowSize : 0d;
                break;
            case NDropdownPlacement.TopStart:
            case NDropdownPlacement.BottomStart:
                left = menuMargin.Left + Clamp(triggerWidth / 2d - ArrowSize / 2d, 12d, Math.Max(12d, menuWidth - 24d));
                top = Placement == NDropdownPlacement.TopStart ? menuMargin.Top + menuHeight - ArrowSize : 0d;
                break;
            case NDropdownPlacement.TopEnd:
            case NDropdownPlacement.BottomEnd:
                left = menuMargin.Left + Clamp(menuWidth - triggerWidth / 2d - ArrowSize / 2d, 12d, Math.Max(12d, menuWidth - 24d));
                top = Placement == NDropdownPlacement.TopEnd ? menuMargin.Top + menuHeight - ArrowSize : 0d;
                break;
            case NDropdownPlacement.Left:
            case NDropdownPlacement.Right:
                left = Placement == NDropdownPlacement.Left ? menuMargin.Left + menuWidth - ArrowSize : 0d;
                top = menuMargin.Top + (menuHeight - ArrowSize) / 2d;
                break;
            case NDropdownPlacement.LeftStart:
            case NDropdownPlacement.RightStart:
                left = Placement == NDropdownPlacement.LeftStart ? menuMargin.Left + menuWidth - ArrowSize : 0d;
                top = menuMargin.Top + Clamp(triggerHeight / 2d - ArrowSize / 2d, 12d, Math.Max(12d, menuHeight - 24d));
                break;
            case NDropdownPlacement.LeftEnd:
            case NDropdownPlacement.RightEnd:
                left = Placement == NDropdownPlacement.LeftEnd ? menuMargin.Left + menuWidth - ArrowSize : 0d;
                top = menuMargin.Top + Clamp(menuHeight - triggerHeight / 2d - ArrowSize / 2d, 12d, Math.Max(12d, menuHeight - 24d));
                break;
            default:
                left = menuMargin.Left + (menuWidth - ArrowSize) / 2d;
                top = 0d;
                break;
        }

        menu.UpdateChromeLayout(true, menuMargin, left, top);
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

    private bool IsPointerWithinMenu()
    {
        return IsSourceWithinMenu(Mouse.DirectlyOver as DependencyObject);
    }

    private bool IsSourceWithinMenu(DependencyObject? source)
    {
        if (source is null)
        {
            return false;
        }

        DependencyObject? current = source;
        while (current is not null)
        {
            if (current is NDropdownMenu { OwnerDropdown: not null } currentMenu && ReferenceEquals(currentMenu.OwnerDropdown, this)
                || current is NDropdownItem { OwnerDropdown: not null } currentItem && ReferenceEquals(currentItem.OwnerDropdown, this))
            {
                return true;
            }

            current = LogicalTreeHelper.GetParent(current)
                      ?? (current is Visual or System.Windows.Media.Media3D.Visual3D
                          ? VisualTreeHelper.GetParent(current)
                          : null);
        }

        return false;
    }
}
