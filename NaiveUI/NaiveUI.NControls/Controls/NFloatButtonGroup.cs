using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NaiveUI.NControls.Tools;

namespace NaiveUI.NControls.Controls;

public class NFloatButtonGroup : ItemsControl
{
    private readonly HashSet<NFloatButton> attachedButtons = [];
    private bool applyingAutoLayout;
    private Thickness userMargin;
    private HorizontalAlignment userHorizontalAlignment;
    private VerticalAlignment userVerticalAlignment;
    private static readonly CornerRadius DefaultCornerRadius = new(8d);

    static NFloatButtonGroup()
    {
        ElementBase.DefaultStyle<NFloatButtonGroup>(DefaultStyleKeyProperty);
    }

    public NFloatButtonGroup()
    {
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;

        userMargin = Margin;
        userHorizontalAlignment = HorizontalAlignment;
        userVerticalAlignment = VerticalAlignment;
    }

    public object? Left
    {
        get => GetValue(LeftProperty);
        set => SetValue(LeftProperty, value);
    }

    public static readonly DependencyProperty LeftProperty =
        ElementBase.Property<NFloatButtonGroup, object?>(nameof(LeftProperty), null, OnLayoutPropertyChanged);

    public object? Right
    {
        get => GetValue(RightProperty);
        set => SetValue(RightProperty, value);
    }

    public static readonly DependencyProperty RightProperty =
        ElementBase.Property<NFloatButtonGroup, object?>(nameof(RightProperty), null, OnLayoutPropertyChanged);

    public object? Top
    {
        get => GetValue(TopProperty);
        set => SetValue(TopProperty, value);
    }

    public static readonly DependencyProperty TopProperty =
        ElementBase.Property<NFloatButtonGroup, object?>(nameof(TopProperty), null, OnLayoutPropertyChanged);

    public object? Bottom
    {
        get => GetValue(BottomProperty);
        set => SetValue(BottomProperty, value);
    }

    public static readonly DependencyProperty BottomProperty =
        ElementBase.Property<NFloatButtonGroup, object?>(nameof(BottomProperty), null, OnLayoutPropertyChanged);

    public NFloatButtonShape Shape
    {
        get => (NFloatButtonShape)GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public static readonly DependencyProperty ShapeProperty =
        ElementBase.Property<NFloatButtonGroup, NFloatButtonShape>(nameof(ShapeProperty), NFloatButtonShape.Circle, OnShapePropertyChanged);

    public NFloatButtonPosition Position
    {
        get => (NFloatButtonPosition)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public static readonly DependencyProperty PositionProperty =
        ElementBase.Property<NFloatButtonGroup, NFloatButtonPosition>(nameof(PositionProperty), NFloatButtonPosition.Fixed, OnLayoutPropertyChanged);

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty =
        ElementBase.Property<NFloatButtonGroup, Orientation>(nameof(OrientationProperty), Orientation.Vertical, OnOrientationPropertyChanged);

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        ElementBase.Property<NFloatButtonGroup, CornerRadius>(nameof(CornerRadiusProperty), DefaultCornerRadius);

    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        RefreshButtonStates();
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

    private static void OnShapePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NFloatButtonGroup group)
        {
            return;
        }

        group.RefreshButtonStates();
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NFloatButtonGroup group)
        {
            return;
        }

        group.UpdateLayoutState();
    }

    private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NFloatButtonGroup group)
        {
            return;
        }

        group.RefreshButtonStates();
        group.InvalidateMeasure();
        group.InvalidateArrange();
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        userMargin = Margin;
        userHorizontalAlignment = HorizontalAlignment;
        userVerticalAlignment = VerticalAlignment;

        RefreshButtonStates();
        UpdateLayoutState();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        foreach (var button in attachedButtons)
        {
            button.SetGroupContext(false, NFloatButtonShape.Circle, Orientation.Vertical, true);
        }

        attachedButtons.Clear();
    }

    private void RefreshButtonStates()
    {
        if (!IsLoaded)
        {
            return;
        }

        var buttons = Items.OfType<NFloatButton>().ToList();

        foreach (var removedButton in attachedButtons.Except(buttons).ToArray())
        {
            removedButton.SetGroupContext(false, NFloatButtonShape.Circle, Orientation.Vertical, true);
            attachedButtons.Remove(removedButton);
        }

        for (var index = 0; index < buttons.Count; index++)
        {
            var button = buttons[index];
            button.SetGroupContext(true, Shape, Orientation, index == buttons.Count - 1);
            attachedButtons.Add(button);
        }
    }

    private void UpdateLayoutState()
    {
        if (!IsLoaded || Position == NFloatButtonPosition.Relative)
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

    private static bool TryResolveOffset(object? value, out double offset)
    {
        return NFloatButton.TryResolveOffset(value, out offset);
    }
}
